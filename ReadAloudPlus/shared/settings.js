const DEFAULT_GLOBAL = {
  sentencePauseMs: 400,
  commaPauseMs: 150,
  otherPauseMs: 250,
  rate: 1,
  pitch: 1,
  voiceAURI: '',
  voiceBURI: '',
  highlight: true,
  jumpOnSelect: true,
  debugLogging: false,
};

const STORAGE_KEY = 'readAloudSettings';

// storage.local is authoritative — it always works, regardless of account/sync policy.
// storage.sync is best-effort on top of it (lets settings follow a signed-in profile),
// but on managed profiles a sync write can silently no-op, so it must never be the only copy.
async function readRaw() {
  try {
    const local = await chrome.storage.local.get(STORAGE_KEY);
    if (local && local[STORAGE_KEY]) return local[STORAGE_KEY];
  } catch {
    // ignore
  }
  try {
    const synced = await chrome.storage.sync.get(STORAGE_KEY);
    if (synced && synced[STORAGE_KEY]) return synced[STORAGE_KEY];
  } catch {
    // ignore
  }
  return null;
}

async function writeRaw(value) {
  await chrome.storage.local.set({ [STORAGE_KEY]: value });
  try {
    await chrome.storage.sync.set({ [STORAGE_KEY]: value });
  } catch {
    // ignore — local write above already succeeded
  }
}

async function loadSettings() {
  const stored = await readRaw();
  return {
    global: { ...DEFAULT_GLOBAL, ...(stored?.global || {}) },
    perSite: stored?.perSite || {},
  };
}

async function saveGlobal(patch) {
  const settings = await loadSettings();
  settings.global = { ...settings.global, ...patch };
  await writeRaw(settings);
  return settings;
}

async function saveForSite(hostname, patch) {
  const settings = await loadSettings();
  settings.perSite[hostname] = { ...(settings.perSite[hostname] || {}), ...patch };
  await writeRaw(settings);
  return settings;
}

async function clearSite(hostname) {
  const settings = await loadSettings();
  delete settings.perSite[hostname];
  await writeRaw(settings);
  return settings;
}

async function resolveForSite(hostname) {
  const settings = await loadSettings();
  return { ...settings.global, ...(settings.perSite[hostname] || {}) };
}

globalThis.ReadAloudSettings = {
  DEFAULT_GLOBAL,
  STORAGE_KEY,
  loadSettings,
  saveGlobal,
  saveForSite,
  clearSite,
  resolveForSite,
};
