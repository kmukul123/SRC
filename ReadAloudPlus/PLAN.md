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
| `popup/popup.{html,css,js}` | Play/pause/stop/skip controls + quick settings |
| `options/options.{html,css,js}` | Global defaults + per-site override management |

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

### Debugging
| What | Where |
| --- | --- |
| Content script logs/errors | F12 DevTools on the page being read → Console |
| Popup logs | Right-click extension icon → **Inspect popup** |
| Service worker logs | `edge://extensions` → click **service worker** on the card |
| Manifest/load errors | Red **Errors** button on the extension card |

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
- [ ] **CP7 — Voice / rate / pitch.** Change each; confirm the next Play uses the new voice, is audibly faster/slower, and higher/lower in pitch.
- [ ] **CP8 — Transport controls.** Pause mid-sentence → speech stops; Play → resumes from the same place. Skip next/prev jumps a segment. Stop ends playback **and clears all highlighting from the page**.
- [ ] **CP9 — DOM is restored.** After Stop, inspect the page in DevTools: no leftover `<span data-rap-wrap>` or `rap-word` elements should remain, and the visible text should be unchanged.
- [ ] **CP10 — Per-site settings.** On site A, tick *Save these settings for this site only*, set a distinctive sentence pause, reload. Site A keeps that value; site B still shows the global default. Confirm site A is listed under **Per-site overrides** in the options page, and that removing it there reverts site A to global defaults.
- [ ] **CP11 — Settings persist.** Close the browser entirely, reopen, and confirm settings survived.
- [ ] **CP12 — Graceful failures.** On `edge://settings` (a restricted page), pressing Play shows the error message in the popup rather than failing silently. On a page with no readable text, the status reads "No readable text found on this page."
- [ ] **CP12b — Context menu.** Right-click directly on a word mid-paragraph → **Read aloud from here** starts at *that word*, not the top or the paragraph start. Right-click on padding/whitespace inside a paragraph → falls back to the paragraph's first segment. Select a passage, right-click → **Read selection aloud** reads only the selection; **Read aloud from here** starts at the first selected word and continues past the selection. **Stop reading aloud** halts playback and clears highlighting.
- [ ] **CP13 — Chrome parity.** Repeat CP1, CP3, CP5 in Chrome.
- [ ] **CP14 — Mac smoke test** (if available). Voice list populates from macOS voices; playback and highlighting work.

## Known limitations (v1)
- Firefox/Safari not supported — they need a different manifest/API surface.
- No cloud TTS (Azure/Google) integration, so no true SSML prosody control; pauses are chunk-and-delay.
- Content extraction is heuristic — unusual page layouts may read navigation text or miss content.
- `speechSynthesis` has a known Chromium quirk where very long utterances can cut off; segmenting per clause largely avoids this.
- **Read aloud from here** resolves the exact word via `caretRangeFromPoint` at right-click time. If you right-click on text that was already read *during the current session* (it's still wrapped in highlight spans, which are only unwrapped on Stop), the anchor can't be matched after the DOM is rebuilt and it degrades to starting at that paragraph. Pressing Stop first restores precision.
- Chromium may suppress speech on a page the user hasn't interacted with yet. If Play appears to do nothing on a freshly opened tab, click anywhere on the page once, then press Play.
