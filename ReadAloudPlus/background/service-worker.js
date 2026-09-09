const INJECTABLE = /^https?:|^file:/;

const MENU_ITEMS = [
  // `page` alone disappears as soon as text is selected, so list every context explicitly.
  { id: 'rap-read-here', title: 'Read aloud from here', contexts: ['page', 'frame', 'image', 'link', 'selection'] },
  { id: 'rap-read-selection', title: 'Read selection aloud', contexts: ['selection'] },
  { id: 'rap-stop', title: 'Stop reading aloud', contexts: ['page', 'frame', 'selection'] },
];

const MENU_COMMANDS = {
  'rap-read-here': 'playFromHere',
  'rap-read-selection': 'playSelection',
  'rap-stop': 'stop',
};

function createMenus() {
  chrome.contextMenus.removeAll(() => {
    for (const item of MENU_ITEMS) chrome.contextMenus.create(item);
  });
}

chrome.runtime.onInstalled.addListener(createMenus);
chrome.runtime.onStartup.addListener(createMenus);

// The content script is declared in the manifest, but pages already open when the
// extension was installed or reloaded won't have it yet — inject it on demand.
async function ensureInjected(tabId, url) {
  if (!url || !INJECTABLE.test(url)) throw new Error('Read Aloud Plus cannot run on this page.');
  await chrome.scripting.insertCSS({ target: { tabId }, files: ['content/reader.css'] });
  await chrome.scripting.executeScript({
    target: { tabId },
    files: ['shared/settings.js', 'content/reader.js'],
  });
}

chrome.contextMenus.onClicked.addListener(async (info, tab) => {
  const command = MENU_COMMANDS[info.menuItemId];
  if (!command || !tab?.id) return;
  try {
    await chrome.tabs.sendMessage(tab.id, { type: command });
  } catch {
    // Content script not present (page predates install) — inject and retry once.
    // The right-click target was never recorded, so fall back to reading from the top.
    await ensureInjected(tab.id, tab.url);
    await chrome.tabs.sendMessage(tab.id, { type: command === 'playFromHere' ? 'play' : command });
  }
});

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message?.type !== 'command') return false;
  (async () => {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    if (!tab) throw new Error('No active tab.');
    await ensureInjected(tab.id, tab.url);
    const response = await chrome.tabs.sendMessage(tab.id, message.payload);
    sendResponse({ ok: true, response, url: tab.url });
  })().catch((error) => sendResponse({ ok: false, error: error.message }));
  return true;
});
