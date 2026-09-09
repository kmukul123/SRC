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

  // Resolve a viewport point to the exact text node + character offset under it.
  function caretAnchorFromPoint(x, y) {
    if (document.caretRangeFromPoint) {
      const range = document.caretRangeFromPoint(x, y);
      if (range) return { node: range.startContainer, offset: range.startOffset };
    } else if (document.caretPositionFromPoint) {
      const position = document.caretPositionFromPoint(x, y);
      if (position) return { node: position.offsetNode, offset: position.offset };
    }
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
      state.lastContextAnchor = state.lastContextRange
        ? { node: state.lastContextRange.startContainer, offset: state.lastContextRange.startOffset }
        : caretAnchorFromPoint(event.clientX, event.clientY);
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

  function buildSegments(settings, range) {
    const root = range ? document.body : findContentRoot();
    const textNodes = collectTextNodes(root, range);
    const segments = [];

    for (const node of textNodes) {
      const raw = node.nodeValue;
      for (const sentence of splitSentences(raw)) {
        if (!sentence.text.trim()) continue;
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
          });
        }
      }
    }
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

  function pickVoice() {
    if (!state.settings.voiceURI) return null;
    return speechSynthesis.getVoices().find((v) => v.voiceURI === state.settings.voiceURI) || null;
  }

  function speakCurrent() {
    if (!state.playing) return;
    if (state.index >= state.segments.length) {
      stop();
      return;
    }

    const segment = state.segments[state.index];
    const wrapper = state.settings.highlight ? wrapSegment(segment) : null;
    if (wrapper) state.activeSegmentEl = wrapper;

    const utterance = new SpeechSynthesisUtterance(segment.text);
    utterance.rate = state.settings.rate;
    utterance.pitch = state.settings.pitch;
    const voice = pickVoice();
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
        if (!state.sawBoundary) startFallbackHighlight(wrapper);
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

  // Segment containing the clicked/selected word, trimmed to start at that word.
  // Returns -1 when the anchor can't be matched (e.g. it points into text that was
  // wrapped for highlighting during an earlier read and no longer exists as-is).
  function indexAtAnchor(anchor) {
    if (!anchor?.node || anchor.node.nodeType !== Node.TEXT_NODE) return -1;
    for (let i = 0; i < state.segments.length; i++) {
      const segment = state.segments[i];
      if (segment.node !== anchor.node) continue;
      const end = segment.nodeStart + segment.text.length;
      if (anchor.offset >= segment.nodeStart && anchor.offset < end) {
        trimToWord(segment, anchor.offset - segment.nodeStart);
        return i;
      }
    }
    return -1;
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
    return 0;
  }

  async function play(options = {}) {
    const anchor = options.fromHere ? state.lastContextAnchor : null;
    const startFrom = options.fromHere ? state.lastContextTarget : null;
    const range = options.selection ? state.lastContextRange : null;

    if (!options.fromHere && !options.selection && state.paused) {
      resume();
      return status();
    }
    stop();
    state.settings = await ReadAloudSettings.resolveForSite(location.hostname);
    state.segments = buildSegments(state.settings, range);

    // Prefer the exact clicked/selected word; fall back to the clicked element's first segment.
    let index = anchor ? indexAtAnchor(anchor) : -1;
    if (index < 0) index = startFrom ? indexAtElement(startFrom) : 0;
    state.index = index;

    if (!state.segments.length) return status();
    state.playing = true;
    state.paused = false;
    speakCurrent();
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
      default:
        return false;
    }
  });

  window.addEventListener('pagehide', stop);
})();
