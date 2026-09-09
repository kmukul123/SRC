(() => {
  if (globalThis.__readAloudPlusLoaded) return;
  globalThis.__readAloudPlusLoaded = true;

  const HIGHLIGHT_CLASS = 'rap-word-active';
  const SENTENCE_CLASS = 'rap-sentence-active';
  const WRAPPER_ATTR = 'data-rap-wrap';

  const SKIP_TAGS = new Set([
    'SCRIPT', 'STYLE', 'NOSCRIPT', 'IFRAME', 'SVG', 'CANVAS', 'AUDIO', 'VIDEO',
    'NAV', 'ASIDE', 'FOOTER', 'HEADER', 'FORM', 'BUTTON', 'SELECT', 'TEXTAREA', 'INPUT',
  ]);

  const state = {
    settings: null,
    segments: [],
    index: 0,
    playing: false,
    paused: false,
    pauseTimer: null,
    fallbackTimer: null,
    boundaryWatchdog: null,
    sawBoundary: false,
    activeWordEl: null,
    activeSegmentEl: null,
    wrappers: [],
    lastContextTarget: null,
    lastContextRange: null,
    lastContextAnchor: null,
  };

  // Diagnostics are off by default — tick "Log diagnostics to the console" in the options
  // page. Read eagerly rather than at play start, because the contextmenu handler runs long
  // before any settings are resolved, and storage.onChanged keeps it live without a reload.
  let debugLogging = false;

  function log(...args) {
    if (debugLogging) console.log('%c[Read Aloud Plus]', 'color:#b45309;font-weight:bold', ...args);
  }

  function preview(text, max = 60) {
    const clean = String(text).replace(/\s+/g, ' ').trim();
    return clean.length > max ? `${clean.slice(0, max)}…` : clean;
  }

  ReadAloudSettings.loadSettings().then(({ global }) => {
    debugLogging = Boolean(global.debugLogging);
    log('content script ready on', location.href);
  });

  chrome.storage.onChanged.addListener((changes) => {
    const next = changes[ReadAloudSettings.STORAGE_KEY]?.newValue?.global?.debugLogging;
    if (next !== undefined) debugLogging = Boolean(next);
  });

  // Resolve a viewport point to the exact text node + character offset under it.
  function caretPointFromPoint(x, y) {
    if (document.caretRangeFromPoint) {
      const range = document.caretRangeFromPoint(x, y);
      if (range) return { node: range.startContainer, offset: range.startOffset };
      log('caretRangeFromPoint returned nothing at', x, y);
    } else if (document.caretPositionFromPoint) {
      const position = document.caretPositionFromPoint(x, y);
      if (position) return { node: position.offsetNode, offset: position.offset };
      log('caretPositionFromPoint returned nothing at', x, y);
    } else {
      log('no caret-from-point API available in this browser');
    }
    return null;
  }

  // Highlight wrappers are torn down and the DOM rebuilt on every (re)start, so a raw
  // text-node reference goes stale. Unwrapping preserves textContent exactly, so an
  // offset measured within a stable ancestor element survives the rebuild instead.
  function stableElementFor(node) {
    let el = node.nodeType === Node.TEXT_NODE ? node.parentElement : node;
    const wrapper = el?.closest?.(`[${WRAPPER_ATTR}]`);
    if (wrapper?.parentElement) el = wrapper.parentElement;
    return el;
  }

  function makeAnchor(node, offset) {
    if (!node || node.nodeType !== Node.TEXT_NODE) {
      log('no anchor: caret landed on an element, not text', node);
      return null;
    }
    const element = stableElementFor(node);
    if (!element) return null;

    const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
    let total = 0;
    let current;
    while ((current = walker.nextNode())) {
      if (current === node) {
        const anchor = { element, offset: total + offset };
        log('anchor', `<${element.tagName.toLowerCase()}>`, `offset ${anchor.offset}`, '→', preview(
          element.textContent.slice(anchor.offset, anchor.offset + 40),
        ));
        return anchor;
      }
      total += current.nodeValue.length;
    }
    log('no anchor: text node not found under', element);
    return null;
  }

  // The contextMenus API reports neither click coordinates nor the target element, so
  // remember them here while the menu is still opening. The caret is resolved now rather
  // than on menu click, while the point is guaranteed to still match what the user saw.
  document.addEventListener(
    'contextmenu',
    (event) => {
      let target = event.target;
      const wrapper = target?.closest?.(`[${WRAPPER_ATTR}]`);
      if (wrapper?.parentElement) target = wrapper.parentElement;
      state.lastContextTarget = target;

      const selection = window.getSelection();
      state.lastContextRange =
        selection && selection.rangeCount && !selection.isCollapsed ? selection.getRangeAt(0).cloneRange() : null;

      // A selection means "start at the first selected word"; otherwise use the click point.
      const point = state.lastContextRange
        ? { node: state.lastContextRange.startContainer, offset: state.lastContextRange.startOffset }
        : caretPointFromPoint(event.clientX, event.clientY);
      state.lastContextAnchor = point ? makeAnchor(point.node, point.offset) : null;
      log(
        'right-click at',
        `${event.clientX},${event.clientY}`,
        state.lastContextRange ? '(using selection start)' : '(using click point)',
        'target',
        target,
      );
    },
    true,
  );

  function isVisible(el) {
    if (!el) return false;
    const style = getComputedStyle(el);
    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') return false;
    if (el.getAttribute('aria-hidden') === 'true') return false;
    return true;
  }

  function findContentRoot() {
    const candidates = ['article', 'main', '[role="main"]', '#content', '.post', '.article-body'];
    for (const selector of candidates) {
      const el = document.querySelector(selector);
      if (el && isVisible(el) && el.innerText && el.innerText.trim().length > 200) return el;
    }
    return document.body;
  }

  function collectTextNodes(root, range) {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
      acceptNode(node) {
        if (!node.nodeValue || !node.nodeValue.trim()) return NodeFilter.FILTER_REJECT;
        const parent = node.parentElement;
        if (!parent || SKIP_TAGS.has(parent.tagName)) return NodeFilter.FILTER_REJECT;
        if (parent.closest(`[${WRAPPER_ATTR}]`)) return NodeFilter.FILTER_REJECT;
        if (!isVisible(parent)) return NodeFilter.FILTER_REJECT;
        if (range && !range.intersectsNode(node)) return NodeFilter.FILTER_REJECT;
        return NodeFilter.FILTER_ACCEPT;
      },
    });
    const nodes = [];
    let node;
    while ((node = walker.nextNode())) nodes.push(node);
    return nodes;
  }

  function splitSentences(text) {
    if (typeof Intl !== 'undefined' && Intl.Segmenter) {
      const segmenter = new Intl.Segmenter(document.documentElement.lang || 'en', { granularity: 'sentence' });
      return [...segmenter.segment(text)].map((s) => ({ text: s.segment, start: s.index }));
    }
    const parts = [];
    const regex = /[^.!?]+[.!?]*\s*/g;
    let match;
    while ((match = regex.exec(text))) parts.push({ text: match[0], start: match.index });
    return parts.length ? parts : [{ text, start: 0 }];
  }

  // The punctuation that ends a segment decides how long we pause after speaking it.
  // Closing quotes/brackets are stripped first so `He said "Stop."` still counts as a
  // sentence ending rather than falling through to the default.
  function pauseAfter(text, settings) {
    const trimmed = text.trimEnd().replace(/["'”’)\]}»]+$/u, '');
    const last = trimmed.slice(-1);
    if (/[.!?…。！？]/u.test(last)) return settings.sentencePauseMs;
    if (/[,、，]/u.test(last)) return settings.commaPauseMs;
    if (/[;:；：—–-]/u.test(last)) return settings.otherPauseMs;
    return settings.otherPauseMs;
  }

  function endsSentence(text) {
    const trimmed = text.trimEnd().replace(/["'”’)\]}»]+$/u, '');
    return /[.!?…。！？]/u.test(trimmed.slice(-1));
  }

  // The content-root heuristic can exclude the very text the user right-clicked (sidebars,
  // comments, pages with a misleading <article>). Widen to <body> rather than silently
  // starting somewhere else entirely.
  function pickRoot(range, focusEl) {
    if (range) return document.body;
    const root = findContentRoot();
    if (focusEl && root !== document.body && !root.contains(focusEl)) {
      log('start point sits outside the detected content root', root, '— widening to <body>');
      return document.body;
    }
    return root;
  }

  function buildSegments(settings, range, focusEl) {
    const root = pickRoot(range, focusEl);
    const textNodes = collectTextNodes(root, range);
    const segments = [];
    // Tracks which sentence each segment belongs to, counted across the whole reading
    // session (not reset per text node), so alternating voices stays in a steady A/B/A/B
    // pattern instead of resetting to voice A at the start of every element.
    let sentenceIndex = 0;
    // Intl.Segmenter only sees one text node's own string, so a sentence split across
    // nodes by inline markup (a link, <b>, <em>, …) looks like several complete sentences
    // to it. If the previous chunk didn't end in terminal punctuation, the next node's
    // first chunk is really a continuation of the same sentence, not a new one.
    let sentenceOpen = false;

    for (const node of textNodes) {
      const raw = node.nodeValue;
      const sentences = splitSentences(raw);
      for (let i = 0; i < sentences.length; i++) {
        const sentence = sentences[i];
        if (!sentence.text.trim()) continue;
        if (!(i === 0 && sentenceOpen)) sentenceIndex++;
        // Sub-split on internal commas/semicolons/colons so each clause can carry its own pause.
        const clauseRegex = /[^,;:]+[,;:]?/g;
        let clause;
        while ((clause = clauseRegex.exec(sentence.text))) {
          const text = clause[0];
          if (!text.trim()) continue;
          segments.push({
            node,
            nodeStart: sentence.start + clause.index,
            text,
            pauseMs: pauseAfter(text, settings),
            sentenceIndex: sentenceIndex - 1,
          });
        }
        sentenceOpen = !endsSentence(sentence.text);
      }
    }
    log(`built ${segments.length} segments from`, root, `(${textNodes.length} text nodes)`);
    return segments;
  }

  function clearHighlight() {
    if (state.activeWordEl) {
      state.activeWordEl.classList.remove(HIGHLIGHT_CLASS);
      state.activeWordEl = null;
    }
    if (state.activeSegmentEl) {
      state.activeSegmentEl.classList.remove(SENTENCE_CLASS);
      state.activeSegmentEl = null;
    }
  }

  function unwrapAll() {
    clearHighlight();
    for (const wrapper of state.wrappers) {
      const parent = wrapper.parentNode;
      if (!parent) continue;
      parent.replaceChild(document.createTextNode(wrapper.textContent), wrapper);
      parent.normalize();
    }
    state.wrappers = [];
  }

  // Replace the segment's slice of its text node with a wrapper of per-word spans,
  // so highlighting can target individual words. Reverted by unwrapAll().
  function wrapSegment(segment) {
    const node = segment.node;
    if (!node.parentNode) return null;

    const value = node.nodeValue;
    const start = segment.nodeStart;
    const end = start + segment.text.length;
    if (start < 0 || end > value.length || value.slice(start, end) !== segment.text) return null;

    const before = value.slice(0, start);
    const after = value.slice(end);

    const wrapper = document.createElement('span');
    wrapper.setAttribute(WRAPPER_ATTR, '');
    wrapper.className = SENTENCE_CLASS;

    const wordRegex = /(\s+)|(\S+)/g;
    let match;
    while ((match = wordRegex.exec(segment.text))) {
      if (match[1]) {
        wrapper.appendChild(document.createTextNode(match[1]));
      } else {
        const span = document.createElement('span');
        span.className = 'rap-word';
        span.dataset.rapStart = String(match.index);
        span.dataset.rapEnd = String(match.index + match[2].length);
        span.textContent = match[2];
        wrapper.appendChild(span);
      }
    }

    const parent = node.parentNode;
    const afterNode = document.createTextNode(after);
    node.nodeValue = before;
    parent.insertBefore(wrapper, node.nextSibling);
    parent.insertBefore(afterNode, wrapper.nextSibling);

    state.wrappers.push(wrapper);
    // Later segments in this same node now live in afterNode, shifted by `end`.
    for (let i = state.index + 1; i < state.segments.length; i++) {
      const later = state.segments[i];
      if (later.node === node) {
        later.node = afterNode;
        later.nodeStart -= end;
      }
    }
    return wrapper;
  }

  function highlightWordAt(wrapper, charIndex) {
    if (!wrapper) return;
    const spans = wrapper.querySelectorAll('.rap-word');
    for (const span of spans) {
      const start = Number(span.dataset.rapStart);
      const end = Number(span.dataset.rapEnd);
      if (charIndex >= start && charIndex < end) {
        if (state.activeWordEl === span) return;
        if (state.activeWordEl) state.activeWordEl.classList.remove(HIGHLIGHT_CLASS);
        span.classList.add(HIGHLIGHT_CLASS);
        state.activeWordEl = span;
        span.scrollIntoView({ block: 'center', behavior: 'smooth' });
        return;
      }
    }
  }

  // Some voices never fire granular `boundary` events; advance the highlight on an
  // estimated per-word timer instead so it doesn't sit frozen on the first word.
  function startFallbackHighlight(wrapper) {
    const spans = [...wrapper.querySelectorAll('.rap-word')];
    if (!spans.length) return;
    const wordsPerSecond = 2.7 * (state.settings.rate || 1);
    const intervalMs = Math.max(120, 1000 / wordsPerSecond);
    let i = 0;
    state.fallbackTimer = setInterval(() => {
      if (i >= spans.length) {
        clearInterval(state.fallbackTimer);
        state.fallbackTimer = null;
        return;
      }
      if (state.activeWordEl) state.activeWordEl.classList.remove(HIGHLIGHT_CLASS);
      spans[i].classList.add(HIGHLIGHT_CLASS);
      state.activeWordEl = spans[i];
      i++;
    }, intervalMs);
  }

  function stopTimers() {
    clearTimeout(state.pauseTimer);
    clearTimeout(state.boundaryWatchdog);
    clearInterval(state.fallbackTimer);
    state.pauseTimer = null;
    state.boundaryWatchdog = null;
    state.fallbackTimer = null;
  }

  // Alternates by sentence: even sentenceIndex speaks with Voice A, odd with Voice B.
  // If both are unset or the same voiceURI, this naturally never actually switches.
  function pickVoiceFor(segment) {
    const uri = segment.sentenceIndex % 2 === 0 ? state.settings.voiceAURI : state.settings.voiceBURI;
    if (!uri) return null;
    return speechSynthesis.getVoices().find((v) => v.voiceURI === uri) || null;
  }

  function speakCurrent() {
    if (!state.playing) return;
    if (state.index >= state.segments.length) {
      stop();
      return;
    }

    const segment = state.segments[state.index];
    // A live settings change re-speaks the same segment without going through onend, so
    // reuse the existing wrapper rather than wrapping an already-wrapped (now truncated) node.
    const wrapper = state.settings.highlight ? state.activeSegmentEl || wrapSegment(segment) : null;
    if (wrapper) state.activeSegmentEl = wrapper;
    else if (state.settings.highlight) log(`segment ${state.index} could not be wrapped — no word highlighting`);
    log(`speak ${state.index}/${state.segments.length} (pause ${segment.pauseMs}ms after) →`, preview(segment.text));

    const utterance = new SpeechSynthesisUtterance(segment.text);
    utterance.rate = state.settings.rate;
    utterance.pitch = state.settings.pitch;
    const voice = pickVoiceFor(segment);
    if (voice) {
      utterance.voice = voice;
      utterance.lang = voice.lang;
    }

    state.sawBoundary = false;
    if (wrapper) {
      utterance.onboundary = (event) => {
        if (event.name && event.name !== 'word') return;
        state.sawBoundary = true;
        clearInterval(state.fallbackTimer);
        state.fallbackTimer = null;
        highlightWordAt(wrapper, event.charIndex);
      };
      state.boundaryWatchdog = setTimeout(() => {
        if (state.sawBoundary) return;
        log('no boundary events from this voice — using the estimated highlight timer');
        startFallbackHighlight(wrapper);
      }, 400);
    }

    utterance.onend = () => {
      stopTimers();
      clearHighlight();
      if (!state.playing) return;
      state.index++;
      state.pauseTimer = setTimeout(speakCurrent, segment.pauseMs);
    };

    utterance.onerror = (event) => {
      if (event.error === 'interrupted' || event.error === 'canceled') return;
      stopTimers();
      state.index++;
      if (state.playing) state.pauseTimer = setTimeout(speakCurrent, segment.pauseMs);
    };

    speechSynthesis.speak(utterance);
  }

  // Drop the leading part of a segment so speech begins at the word containing `offset`.
  function trimToWord(segment, offset) {
    const text = segment.text;
    let start = Math.max(0, Math.min(offset, text.length - 1));
    if (/\s/.test(text[start])) {
      while (start < text.length && /\s/.test(text[start])) start++;
    } else {
      while (start > 0 && !/\s/.test(text[start - 1])) start--;
    }
    if (start <= 0 || start >= text.length) return;
    segment.nodeStart += start;
    segment.text = text.slice(start);
  }

  // Segment containing the anchored word, trimmed to start at that word. Returns -1 when
  // the anchor can't be matched at all, so the caller can fall back to the clicked element.
  function indexAtAnchor(anchor) {
    if (!anchor?.element) return -1;
    if (!document.contains(anchor.element)) {
      log('anchor element is no longer in the document — the page changed since the right-click');
      return -1;
    }

    const walker = document.createTreeWalker(anchor.element, NodeFilter.SHOW_TEXT);
    const baseOffsets = new Map();
    let total = 0;
    let node;
    while ((node = walker.nextNode())) {
      baseOffsets.set(node, total);
      total += node.nodeValue.length;
    }

    let following = -1;
    let candidates = 0;
    for (let i = 0; i < state.segments.length; i++) {
      const segment = state.segments[i];
      const base = baseOffsets.get(segment.node);
      if (base === undefined) continue;
      candidates++;
      const start = base + segment.nodeStart;
      if (anchor.offset >= start && anchor.offset < start + segment.text.length) {
        trimToWord(segment, anchor.offset - start);
        log(`anchor matched segment ${i} →`, preview(segment.text));
        return i;
      }
      // Anchor landed between segments (whitespace, skipped markup) — take the next one.
      if (following < 0 && start >= anchor.offset) following = i;
    }

    if (!candidates) {
      log('anchor element holds no readable segments — it was skipped during extraction');
    } else if (following >= 0) {
      log(`anchor fell between segments; using the next one (${following}) →`, preview(state.segments[following].text));
    } else {
      log(`anchor offset ${anchor.offset} is past all ${candidates} segments in its element`);
    }
    return following;
  }

  // First segment at or after the right-clicked element, in document order.
  function indexAtElement(el) {
    if (!el) return 0;
    for (let i = 0; i < state.segments.length; i++) {
      const parent = state.segments[i].node.parentElement;
      if (!parent) continue;
      if (el === parent || el.contains(parent)) return i;
      if (el.compareDocumentPosition(parent) & Node.DOCUMENT_POSITION_FOLLOWING) return i;
    }
    // Nothing follows the click, so it sits below the last readable text. Starting at the
    // very top would be the most surprising possible answer; start at the end instead.
    log('no segment at or after the clicked element — starting at the last segment');
    return Math.max(0, state.segments.length - 1);
  }

  async function play(options = {}) {
    const anchor = options.anchor || (options.fromHere ? state.lastContextAnchor : null);
    const startFrom = options.fromHere ? state.lastContextTarget : null;
    const range = options.selection ? state.lastContextRange : null;

    if (!options.fromHere && !options.selection && state.paused) {
      resume();
      return status();
    }
    stop();
    state.settings = await ReadAloudSettings.resolveForSite(location.hostname);
    debugLogging = Boolean(state.settings.debugLogging);
    log('play', { fromHere: Boolean(options.fromHere), selection: Boolean(options.selection), anchor: Boolean(anchor) });
    state.segments = buildSegments(state.settings, range, anchor?.element || startFrom);

    // Prefer the exact clicked/selected word; fall back to the clicked element's first segment.
    let index = anchor ? indexAtAnchor(anchor) : -1;
    if (index < 0 && startFrom) {
      log('anchor unusable — falling back to the clicked element');
      index = indexAtElement(startFrom);
    }
    if (index < 0) index = 0;
    state.index = index;

    if (!state.segments.length) {
      log('no readable text found');
      return status();
    }
    log(`starting at segment ${index} of ${state.segments.length} →`, preview(state.segments[index]?.text));
    state.playing = true;
    state.paused = false;
    speakCurrent();
    return status();
  }

  // Voice/rate/pitch are baked into a SpeechSynthesisUtterance when it's created and can't
  // be changed while it's speaking — so apply a live change by re-speaking the same segment
  // from its start with the new utterance settings, instead of waiting for the next Play.
  async function refreshSettings() {
    const previous = state.settings;
    state.settings = await ReadAloudSettings.resolveForSite(location.hostname);
    debugLogging = Boolean(state.settings.debugLogging);

    const shouldRestart =
      state.playing &&
      !state.paused &&
      previous &&
      (previous.rate !== state.settings.rate ||
        previous.pitch !== state.settings.pitch ||
        previous.voiceAURI !== state.settings.voiceAURI ||
        previous.voiceBURI !== state.settings.voiceBURI);

    if (shouldRestart) {
      log('voice/rate/pitch changed while reading — restarting the current segment with the new settings');
      stopTimers();
      if (state.activeWordEl) {
        state.activeWordEl.classList.remove(HIGHLIGHT_CLASS);
        state.activeWordEl = null;
      }
      speechSynthesis.cancel();
      speakCurrent();
    }
    return status();
  }

  function pause() {
    if (!state.playing) return status();
    state.paused = true;
    clearInterval(state.fallbackTimer);
    state.fallbackTimer = null;
    speechSynthesis.pause();
    return status();
  }

  function resume() {
    if (!state.paused) return status();
    state.paused = false;
    speechSynthesis.resume();
    return status();
  }

  function stop() {
    state.playing = false;
    state.paused = false;
    stopTimers();
    speechSynthesis.cancel();
    unwrapAll();
    state.segments = [];
    state.index = 0;
    return status();
  }

  function skip(delta) {
    if (!state.segments.length) return status();
    stopTimers();
    clearHighlight();
    speechSynthesis.cancel();
    state.index = Math.max(0, Math.min(state.segments.length - 1, state.index + delta));
    state.paused = false;
    if (state.playing) speakCurrent();
    return status();
  }

  function status() {
    return {
      playing: state.playing,
      paused: state.paused,
      index: state.index,
      total: state.segments.length,
      hostname: location.hostname,
    };
  }

  chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
    switch (message?.type) {
      case 'play':
        play().then(sendResponse);
        return true;
      case 'playFromHere':
        play({ fromHere: true }).then(sendResponse);
        return true;
      case 'playSelection':
        play({ selection: true }).then(sendResponse);
        return true;
      case 'pause':
        sendResponse(pause());
        return false;
      case 'resume':
        sendResponse(resume());
        return false;
      case 'stop':
        sendResponse(stop());
        return false;
      case 'skipNext':
        sendResponse(skip(1));
        return false;
      case 'skipPrev':
        sendResponse(skip(-1));
        return false;
      case 'status':
        sendResponse(status());
        return false;
      case 'refreshSettings':
        refreshSettings().then(sendResponse);
        return true;
      default:
        return false;
    }
  });

  // Selecting text mid-read jumps playback to the first selected word.
  document.addEventListener(
    'mouseup',
    () => {
      if (!state.playing || !state.settings?.jumpOnSelect) return;
      // Selection isn't finalised until after the mouseup handlers run.
      setTimeout(() => {
        if (!state.playing) return;
        const selection = window.getSelection();
        if (!selection || !selection.rangeCount || selection.isCollapsed) return;

        const range = selection.getRangeAt(0);
        const container =
          range.startContainer.nodeType === Node.TEXT_NODE ? range.startContainer.parentElement : range.startContainer;
        // Don't hijack selections inside form fields or editors.
        if (container?.closest?.('input, textarea, [contenteditable=""], [contenteditable="true"]')) return;

        const anchor = makeAnchor(range.startContainer, range.startOffset);
        if (anchor) {
          log('selection made while reading — jumping to it');
          play({ anchor });
        }
      }, 0);
    },
    true,
  );

  window.addEventListener('pagehide', stop);
})();
