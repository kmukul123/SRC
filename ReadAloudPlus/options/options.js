const FIELDS = ['sentencePauseMs', 'commaPauseMs', 'otherPauseMs'];
const els = {
  voiceA: document.getElementById('voiceA'),
  voiceB: document.getElementById('voiceB'),
  rate: document.getElementById('rate'),
  rateOut: document.getElementById('rateOut'),
  pitch: document.getElementById('pitch'),
  pitchOut: document.getElementById('pitchOut'),
  highlight: document.getElementById('highlight'),
  jumpOnSelect: document.getElementById('jumpOnSelect'),
  debugLogging: document.getElementById('debugLogging'),
  voiceHint: document.getElementById('voiceHint'),
  saved: document.getElementById('saved'),
  siteList: document.getElementById('siteList'),
  noSites: document.getElementById('noSites'),
  newSiteHost: document.getElementById('newSiteHost'),
  addSite: document.getElementById('addSite'),
};
const pauseEls = Object.fromEntries(FIELDS.map((id) => [id, document.getElementById(id)]));

function fillVoiceSelect(select, selectedURI, voices) {
  select.innerHTML = '';
  select.add(new Option('Browser default', ''));
  for (const voice of voices) {
    select.add(new Option(`${voice.name} (${voice.lang})${voice.localService ? '' : ' — online'}`, voice.voiceURI));
  }
  select.value = voices.some((v) => v.voiceURI === selectedURI) ? selectedURI : '';
}

function populateVoices(selectedAURI, selectedBURI) {
  const voices = speechSynthesis.getVoices();
  fillVoiceSelect(els.voiceA, selectedAURI, voices);
  fillVoiceSelect(els.voiceB, selectedBURI, voices);

  const hasNatural = voices.some((v) => /natural|neural|premium|enhanced|online/i.test(v.name));
  if (voices.length && !hasNatural) {
    els.voiceHint.hidden = false;
    els.voiceHint.textContent = navigator.userAgent.includes('Mac')
      ? 'Only basic voices are installed. Add higher-quality voices in System Settings → Accessibility → Spoken Content → System Voice → Manage Voices, then restart the browser.'
      : 'Only basic voices are installed. Add natural voices in Windows Settings → Time & Language → Speech → Add voices, then restart the browser.';
  }
}

function makeVoiceSelect(selectedURI) {
  const select = document.createElement('select');
  fillVoiceSelect(select, selectedURI, speechSynthesis.getVoices());
  return select;
}

function labeled(text, ...controls) {
  const label = document.createElement('label');
  label.append(text, ...controls);
  return label;
}

async function renderSites() {
  const { global, perSite } = await ReadAloudSettings.loadSettings();
  const hosts = Object.keys(perSite).sort();
  els.siteList.innerHTML = '';
  els.noSites.hidden = hosts.length > 0;

  for (const host of hosts) {
    const effective = { ...global, ...perSite[host] };

    const li = document.createElement('li');
    li.className = 'site-row';

    const header = document.createElement('div');
    header.className = 'site-row-header';
    const name = document.createElement('strong');
    name.textContent = host;
    const remove = document.createElement('button');
    remove.textContent = 'Remove';
    remove.addEventListener('click', async () => {
      await ReadAloudSettings.clearSite(host);
      renderSites();
    });
    header.append(name, remove);

    const voiceA = makeVoiceSelect(effective.voiceAURI);
    voiceA.addEventListener('change', () => ReadAloudSettings.saveForSite(host, { voiceAURI: voiceA.value }));

    const voiceB = makeVoiceSelect(effective.voiceBURI);
    voiceB.addEventListener('change', () => ReadAloudSettings.saveForSite(host, { voiceBURI: voiceB.value }));

    const rate = document.createElement('input');
    rate.type = 'range';
    rate.min = '0.5';
    rate.max = '2';
    rate.step = '0.05';
    rate.value = effective.rate;
    const rateOut = document.createElement('output');
    rateOut.textContent = `${Number(effective.rate).toFixed(2)}x`;
    rate.addEventListener('input', () => {
      rateOut.textContent = `${Number(rate.value).toFixed(2)}x`;
    });
    rate.addEventListener('change', () => ReadAloudSettings.saveForSite(host, { rate: Number(rate.value) }));

    const pitch = document.createElement('input');
    pitch.type = 'range';
    pitch.min = '0.5';
    pitch.max = '2';
    pitch.step = '0.05';
    pitch.value = effective.pitch;
    const pitchOut = document.createElement('output');
    pitchOut.textContent = Number(effective.pitch).toFixed(2);
    pitch.addEventListener('input', () => {
      pitchOut.textContent = Number(pitch.value).toFixed(2);
    });
    pitch.addEventListener('change', () => ReadAloudSettings.saveForSite(host, { pitch: Number(pitch.value) }));

    const controls = document.createElement('div');
    controls.className = 'site-controls';
    controls.append(
      labeled('Voice A', voiceA),
      labeled('Voice B', voiceB),
      labeled('Speed', rate, rateOut),
      labeled('Pitch', pitch, pitchOut),
    );

    li.append(header, controls);
    els.siteList.appendChild(li);
  }
}

els.addSite.addEventListener('click', async () => {
  const host = els.newSiteHost.value.trim().toLowerCase();
  if (!host) return;
  const { global } = await ReadAloudSettings.loadSettings();
  await ReadAloudSettings.saveForSite(host, {
    voiceAURI: global.voiceAURI,
    voiceBURI: global.voiceBURI,
    rate: global.rate,
    pitch: global.pitch,
  });
  els.newSiteHost.value = '';
  renderSites();
});

async function load() {
  const { global } = await ReadAloudSettings.loadSettings();
  els.rate.value = global.rate;
  els.rateOut.value = `${Number(global.rate).toFixed(2)}x`;
  els.pitch.value = global.pitch;
  els.pitchOut.value = Number(global.pitch).toFixed(2);
  els.highlight.checked = global.highlight;
  els.jumpOnSelect.checked = global.jumpOnSelect;
  els.debugLogging.checked = global.debugLogging;
  for (const field of FIELDS) pauseEls[field].value = global[field];
  populateVoices(global.voiceAURI, global.voiceBURI);
  renderSites();
}

async function persist() {
  const patch = {
    rate: Number(els.rate.value),
    pitch: Number(els.pitch.value),
    voiceAURI: els.voiceA.value,
    voiceBURI: els.voiceB.value,
    highlight: els.highlight.checked,
    jumpOnSelect: els.jumpOnSelect.checked,
    debugLogging: els.debugLogging.checked,
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

for (const el of [
  els.rate,
  els.pitch,
  els.voiceA,
  els.voiceB,
  els.highlight,
  els.jumpOnSelect,
  els.debugLogging,
  ...Object.values(pauseEls),
]) {
  el.addEventListener('change', persist);
}
els.rate.addEventListener('input', () => {
  els.rateOut.value = `${Number(els.rate.value).toFixed(2)}x`;
});
els.pitch.addEventListener('input', () => {
  els.pitchOut.value = Number(els.pitch.value).toFixed(2);
});

speechSynthesis.addEventListener('voiceschanged', () => {
  populateVoices(els.voiceA.value, els.voiceB.value);
  renderSites();
});
load();
