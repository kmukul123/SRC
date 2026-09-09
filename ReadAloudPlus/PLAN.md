# Read Aloud Plus — Plan, Install & Test Checkpoints

## What this is
A Manifest V3 browser extension for Edge and Chrome (Windows + Mac) that reads the current page
aloud like Edge's built-in Read Aloud, but with settings that Read Aloud doesn't expose:
configurable pause lengths at sentence, comma and other punctuation breaks, plus voice/rate/pitch,
word-by-word highlighting, and per-site preferences.

## Why it's built this way
Edge's native Read Aloud is a private, built-in browser UI feature — there is no extension API to
invoke or customise it. So this is an independent reader built on the standard Web Speech API
(`speechSynthesis` / `SpeechSynthesisUtterance`), which works identically in Edge and Chrome on
Windows, Mac and Linux.

Two consequences worth knowing:
- **Pauses**: the Web Speech API has no inline SSML `<break>` support, so pauses are produced by
  splitting text into clause-sized chunks and inserting a `setTimeout` gap between utterances.
- **Voice quality depends on the OS, not this code.** Windows exposes Microsoft's neural "Natural"
  voices only if installed; Mac exposes its system voices, with Enhanced/Premium available on
  download. The extension detects a low-quality-only voice list and shows a hint.

## File map
| File | Role |
| --- | --- |
| `manifest.json` | MV3 manifest — `storage`, `activeTab`, `scripting` permissions |
| `background/service-worker.js` | Context menu items, on-demand injection fallback, routes popup → tab messages |
| `content/reader.js` | Core engine: text extraction, segmentation, playback queue, highlighting |
| `content/reader.css` | Word/sentence highlight styles (light + dark) |
| `shared/settings.js` | Storage wrapper: global defaults, per-site overrides, resolution |
| `popup/popup.{html,css,js}` | Play/pause/stop/skip transport controls + quick voice/speed/pitch |
| `options/options.{html,css,js}` | Global defaults, pauses, diagnostics, and per-site override create/edit/remove |

## How the engine works (`content/reader.js`)
1. **Find content** — prefers `<article>`, `<main>`, `[role=main]`, falls back to `<body>`.
2. **Collect text** — `TreeWalker` over text nodes, skipping `script`/`style`/`nav`/`aside`/hidden elements.
3. **Segment** — `Intl.Segmenter` (sentence granularity) with a regex fallback, then a secondary
   split on `,` `;` `:` so each clause carries its own pause duration.
4. **Wrap words** — the current segment's text is swapped for a `<span>` of per-word spans so a
   word can be highlighted; the original DOM is restored on stop (`unwrapAll`).
5. **Speak** — one utterance per clause (not per word — per-word utterances destroy intonation),
   with the configured pause inserted via `setTimeout` between them.
6. **Highlight** — `boundary` events map `charIndex` → word span. If no boundary event arrives
   within 400 ms (some voices don't fire them), a rate-derived timer advances the highlight instead.
7. **Live voice/rate/pitch changes** — `SpeechSynthesisUtterance` properties can't change once
   speaking has started, so changing Speed/Pitch/Voice in the popup while reading cancels and
   immediately re-speaks the current segment from its start with the new settings, rather than
   waiting for the next Play. (Changing these from the Options page still only applies next Play —
   the options page has no reliable way to know which tab is currently reading.)

## Installing locally to test

### Edge
1. Open `edge://extensions`.
2. Turn on **Developer mode** (toggle, bottom-left).
3. Click **Load unpacked** and select the folder `C:\SRC\temp\readaloud` — select the *folder*, not `manifest.json`.
4. Pin the icon: click the puzzle-piece Extensions button in the toolbar → pin "Read Aloud Plus".

### Chrome
Identical, starting at `chrome://extensions`.

### Reloading after code changes
- Changes to `content/` or `background/`: click the **circular reload arrow** on the extension card, **then reload the web page** you're testing.
- Changes to `popup/` or `options/`: just close and reopen the popup / options page.

> **Always reload the page after reloading the extension.** Reloading the extension orphans the
> content script already running in open tabs, so the right-click listener that records *where* you
> clicked is gone. The service worker recovers by injecting a fresh copy, but that copy never saw the
> right-click — so **"Read aloud from here" silently degrades to reading from the top of the page.**
> This is the most common cause of "it started in the wrong place".

### Debugging
There are three separate consoles — extension code doesn't all log to one place:

| What | Where |
| --- | --- |
| Content script logs/errors (the reader engine) | F12 DevTools on the page being read → Console |
| Popup logs | Right-click extension icon → **Inspect popup** |
| Service worker logs (context menus, injection) | `edge://extensions` → click **service worker** on the card |
| Manifest/load errors | Red **Errors** button on the extension card |

#### Diagnostic logging
The extension's own logging is off by default. Turn on **Log diagnostics to the console** under
*Diagnostics* in the options page — it applies immediately, no reload needed. Page-side lines are
prefixed `[Read Aloud Plus]`, background lines `[Read Aloud Plus SW]`.

With it on, a right-click → **Read aloud from here** should produce roughly:

```
[Read Aloud Plus] right-click at 412,633 (using click point) target <p>
[Read Aloud Plus] anchor <p> offset 148 → "considerable difficulty in reconciling…"
[Read Aloud Plus SW] menu click rap-read-here → playFromHere
[Read Aloud Plus] play {fromHere: true, selection: false, anchor: true}
[Read Aloud Plus] built 214 segments from <article> (61 text nodes)
[Read Aloud Plus] anchor matched segment 87 → "considerable difficulty in reconciling…"
[Read Aloud Plus] starting at segment 87 of 214 → "considerable difficulty in reconciling…"
```

What the failure modes look like:

| Log line | Meaning |
| --- | --- |
| `content script missing, injecting and retrying` | The page predates the extension reload — reload the page; the click point was lost, so it reads from the top. |
| `no anchor: caret landed on an element, not text` | Clicked on padding/margin rather than a word — falls back to the clicked element's first segment. |
| `anchor element holds no readable segments` | The clicked text was skipped during extraction (a `SKIP_TAGS` element, hidden, or inside an existing highlight wrapper). |
| `start point sits outside the detected content root — widening to <body>` | The `<article>`/`<main>` heuristic picked the wrong container; handled automatically. |
| `no segment at or after the clicked element` | Clicked below all readable text — starts at the last segment. |
| `no boundary events from this voice` | The chosen voice doesn't report word positions; the estimated highlight timer is driving highlighting. |

### Voice quality prerequisite
If the voice dropdown only lists robotic voices, the extension shows a hint. To fix:
- **Windows**: Settings → Time & Language → Speech → **Add voices** (or Settings → Accessibility → Narrator → Add natural voices).
- **Mac**: System Settings → Accessibility → Spoken Content → System Voice → **Manage Voices** → download an *Enhanced* or *Premium* voice.

Restart the browser after installing voices before they appear in the list.

## Test checkpoints
Work through these in order — each one isolates a different layer, so a failure tells you where to look.

- [ ] **CP1 — Extension loads.** Card appears on `edge://extensions` with no red **Errors** button; icon is visible in the toolbar.
- [ ] **CP2 — Popup renders.** Clicking the icon opens the popup; the voice dropdown is populated with system voices (not empty). If empty, reopen the popup — voices load asynchronously via `voiceschanged`.
- [ ] **CP3 — Basic playback.** On a Wikipedia article, click **Play**. Speech starts, and the status line shows `Reading — segment N of M`.
- [ ] **CP4 — Word highlighting.** While reading, individual words highlight in sync with the speech and the page auto-scrolls to follow. Try both a local voice and an online/neural voice — if a voice doesn't fire `boundary` events, the timer fallback should still advance the highlight rather than freezing on word one.
- [ ] **CP5 — Sentence pause works.** Set *Sentence pause* to `1500` ms, press Play, and confirm audibly longer gaps at full stops. Set it to `0` and confirm the gaps disappear.
- [ ] **CP6 — Comma pause works.** Set *Comma pause* to `1200` ms and read a comma-heavy paragraph; confirm mid-sentence gaps lengthen independently of sentence pauses.
- [ ] **CP7 — Voice / rate / pitch.** While reading, change Speed in the popup — the current segment restarts immediately, audibly faster/slower, without needing Stop/Play. Same for Pitch and Voice. Word highlighting keeps working on the restarted segment.
- [ ] **CP8 — Transport controls.** Pause mid-sentence → speech stops; Play → resumes from the same place. Skip next/prev jumps a segment. Stop ends playback **and clears all highlighting from the page**.
- [ ] **CP9 — DOM is restored.** After Stop, inspect the page in DevTools: no leftover `<span data-rap-wrap>` or `rap-word` elements should remain, and the visible text should be unchanged.
- [ ] **CP10 — Per-site settings.** In the options page, under **Per-site overrides**, type site A's hostname and click *Add site override* (it starts from the current global voice/speed/pitch). Change its voice or speed there and reload site A — it uses the override; site B still uses the global default. Confirm the popup's voice/speed/pitch controls read/write that same override when you're on site A (its *Add site override* row updates live). Remove the override in the options page — site A reverts to global defaults, and the popup on site A now edits the global settings again.
- [ ] **CP11 — Settings persist.** Close the browser entirely, reopen, and confirm settings survived.
- [ ] **CP12 — Graceful failures.** On `edge://settings` (a restricted page), pressing Play shows the error message in the popup rather than failing silently. On a page with no readable text, the status reads "No readable text found on this page."
- [ ] **CP12b — Context menu.** Right-click directly on a word mid-paragraph → **Read aloud from here** starts at *that word*, not the top or the paragraph start. Right-click on padding/whitespace inside a paragraph → falls back to the paragraph's first segment. Select a passage, right-click → **Read selection aloud** reads only the selection; **Read aloud from here** starts at the first selected word and continues past the selection. **Stop reading aloud** halts playback and clears highlighting.
- [ ] **CP12c — Select-to-jump.** While reading, double-click a word further down the page → playback restarts from that word. Select a phrase → playback jumps to its first word. Select text inside a form field or search box → playback is *not* hijacked. Untick *Selecting text jumps reading to it* → selecting text no longer affects playback (settings are read at play start, so press Stop then Play after changing it).
- [ ] **CP12d — Diagnostic logging.** Tick *Log diagnostics to the console* in the options page (no reload). On a page's DevTools console, right-click a word → **Read aloud from here** and confirm the `right-click at … / anchor … / starting at segment …` lines appear and the quoted text matches the word you clicked. Untick it and confirm the console goes quiet.
- [ ] **CP13 — Chrome parity.** Repeat CP1, CP3, CP5 in Chrome.
- [ ] **CP14 — Mac smoke test** (if available). Voice list populates from macOS voices; playback and highlighting work.

## Known limitations (v1)
- Firefox/Safari not supported — they need a different manifest/API surface.
- No cloud TTS (Azure/Google) integration, so no true SSML prosody control; pauses are chunk-and-delay.
- Content extraction is heuristic — unusual page layouts may read navigation text or miss content.
- `speechSynthesis` has a known Chromium quirk where very long utterances can cut off; segmenting per clause largely avoids this.
- Chromium may suppress speech on a page the user hasn't interacted with yet. If Play appears to do nothing on a freshly opened tab, click anywhere on the page once, then press Play.
