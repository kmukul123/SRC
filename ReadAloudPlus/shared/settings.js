const DEFAULT_GLOBAL = {
  sentencePauseMs: 400,
  commaPauseMs: 150,
  otherPauseMs: 250,
  rate: 1,
  pitch: 1,
  voiceURI: '',
  highlight: true,
  jumpOnSelect: true,
};

const STORAGE_KEY = 'readAloudSettings';

// storage.sync can be unavailable (managed profiles) or over quota; fall back to local.
async function readRaw() {
  for (const area of [chrome.storage.sync, chrome.storage.local]) {
    if (!area) continue;
    try {
      const result = await area.get(STORAGE_KEY);
      if (result && result[STORAGE_KEY]) return result[STORAGE_KEY];
    } catch {
      continue;
    }
  }
  return null;
}

async function writeRaw(value) {
  try {
    await chrome.storage.sync.set({ [STORAGE_KEY]: value });
  } catch {
    await chrome.storage.local.set({ [STORAGE_KEY]: value });
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
  loadSettings,
  saveGlobal,
  saveForSite,
  clearSite,
  resolveForSite,
};
