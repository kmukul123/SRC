const FIELDS = ['sentencePauseMs', 'commaPauseMs', 'otherPauseMs'];
const els = {
  voice: document.getElementById('voice'),
  rate: document.getElementById('rate'),
  rateOut: document.getElementById('rateOut'),
  pitch: document.getElementById('pitch'),
  pitchOut: document.getElementById('pitchOut'),
  highlight: document.getElementById('highlight'),
  jumpOnSelect: document.getElementById('jumpOnSelect'),
  voiceHint: document.getElementById('voiceHint'),
  saved: document.getElementById('saved'),
  siteList: document.getElementById('siteList'),
  noSites: document.getElementById('noSites'),
};
const pauseEls = Object.fromEntries(FIELDS.map((id) => [id, document.getElementById(id)]));

function populateVoices(selectedURI) {
  const voices = speechSynthesis.getVoices();
  els.voice.innerHTML = '';
  els.voice.add(new Option('Browser default', ''));
  for (const voice of voices) {
    els.voice.add(new Option(`${voice.name} (${voice.lang})${voice.localService ? '' : ' — online'}`, voice.voiceURI));
  }
  els.voice.value = voices.some((v) => v.voiceURI === selectedURI) ? selectedURI : '';

  const hasNatural = voices.some((v) => /natural|neural|premium|enhanced|online/i.test(v.name));
  if (voices.length && !hasNatural) {
    els.voiceHint.hidden = false;
    els.voiceHint.textContent = navigator.userAgent.includes('Mac')
      ? 'Only basic voices are installed. Add higher-quality voices in System Settings → Accessibility → Spoken Content → System Voice → Manage Voices, then restart the browser.'
      : 'Only basic voices are installed. Add natural voices in Windows Settings → Time & Language → Speech → Add voices, then restart the browser.';
  }
}

function describe(override) {
  return Object.entries(override)
    .map(([key, value]) => `${key}: ${value === '' ? 'default' : value}`)
    .join(', ');
}

async function renderSites() {
  const { perSite } = await ReadAloudSettings.loadSettings();
  const hosts = Object.keys(perSite).sort();
  els.siteList.innerHTML = '';
  els.noSites.hidden = hosts.length > 0;

  for (const host of hosts) {
    const li = document.createElement('li');
    const info = document.createElement('div');
    const name = document.createElement('strong');
    name.textContent = host;
    const detail = document.createElement('div');
    detail.className = 'site-detail';
    detail.textContent = describe(perSite[host]);
    info.append(name, detail);

    const remove = document.createElement('button');
    remove.textContent = 'Remove';
    remove.addEventListener('click', async () => {
      await ReadAloudSettings.clearSite(host);
      renderSites();
    });

    li.append(info, remove);
    els.siteList.appendChild(li);
  }
}

async function load() {
  const { global } = await ReadAloudSettings.loadSettings();
  els.rate.value = global.rate;
  els.rateOut.value = `${Number(global.rate).toFixed(2)}x`;
  els.pitch.value = global.pitch;
  els.pitchOut.value = Number(global.pitch).toFixed(2);
  els.highlight.checked = global.highlight;
  els.jumpOnSelect.checked = global.jumpOnSelect;
  for (const field of FIELDS) pauseEls[field].value = global[field];
  populateVoices(global.voiceURI);
  renderSites();
}

async function persist() {
  const patch = {
    rate: Number(els.rate.value),
    pitch: Number(els.pitch.value),
    voiceURI: els.voice.value,
    highlight: els.highlight.checked,
    jumpOnSelect: els.jumpOnSelect.checked,
  };
  for (const field of FIELDS) patch[field] = Number(pauseEls[field].value);
  await ReadAloudSettings.saveGlobal(patch);
  els.rateOut.value = `${patch.rate.toFixed(2)}x`;
  els.pitchOut.value = patch.pitch.toFixed(2);
  els.saved.hidden = false;
  setTimeout(() => {
    els.saved.hidden = true;
  }, 1500);
}

for (const el of [els.rate, els.pitch, els.voice, els.highlight, els.jumpOnSelect, ...Object.values(pauseEls)]) {
  el.addEventListener('change', persist);
}
els.rate.addEventListener('input', () => {
  els.rateOut.value = `${Number(els.rate.value).toFixed(2)}x`;
});
els.pitch.addEventListener('input', () => {
  els.pitchOut.value = Number(els.pitch.value).toFixed(2);
});

speechSynthesis.addEventListener('voiceschanged', () => populateVoices(els.voice.value));
load();
