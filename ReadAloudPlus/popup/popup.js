const els = {
  status: document.getElementById('status'),
  voice: document.getElementById('voice'),
  rate: document.getElementById('rate'),
  rateOut: document.getElementById('rateOut'),
  pitch: document.getElementById('pitch'),
  pitchOut: document.getElementById('pitchOut'),
  highlight: document.getElementById('highlight'),
  jumpOnSelect: document.getElementById('jumpOnSelect'),
  perSite: document.getElementById('perSite'),
  host: document.getElementById('host'),
  voiceHint: document.getElementById('voiceHint'),
};

const PAUSE_FIELDS = ['sentencePauseMs', 'commaPauseMs', 'otherPauseMs'];
const pauseEls = Object.fromEntries(PAUSE_FIELDS.map((id) => [id, document.getElementById(id)]));

let hostname = '';

function send(payload) {
  return chrome.runtime.sendMessage({ type: 'command', payload });
}

function setStatus(text) {
  els.status.textContent = text;
}

async function currentHostname() {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  try {
    return new URL(tab.url).hostname;
  } catch {
    return '';
  }
}

function populateVoices(selectedURI) {
  const voices = speechSynthesis.getVoices();
  els.voice.innerHTML = '';
  const auto = new Option('Browser default', '');
  els.voice.add(auto);
  for (const voice of voices) {
    const label = `${voice.name} (${voice.lang})${voice.localService ? '' : ' — online'}`;
    els.voice.add(new Option(label, voice.voiceURI));
  }
  els.voice.value = voices.some((v) => v.voiceURI === selectedURI) ? selectedURI : '';

  const hasNatural = voices.some((v) => /natural|neural|premium|enhanced|online/i.test(v.name));
  if (voices.length && !hasNatural) {
    els.voiceHint.hidden = false;
    els.voiceHint.textContent = navigator.userAgent.includes('Mac')
      ? 'Only basic voices found. Add better ones in System Settings → Accessibility → Spoken Content → Manage Voices.'
      : 'Only basic voices found. Add natural voices in Windows Settings → Time & Language → Speech → Add voices.';
  }
}

async function loadSettings() {
  hostname = await currentHostname();
  els.host.textContent = hostname || 'this site';

  const all = await ReadAloudSettings.loadSettings();
  const hasOverride = Boolean(all.perSite[hostname]);
  els.perSite.checked = hasOverride;

  const effective = { ...all.global, ...(all.perSite[hostname] || {}) };
  els.rate.value = effective.rate;
  els.rateOut.value = `${Number(effective.rate).toFixed(2)}x`;
  els.pitch.value = effective.pitch;
  els.pitchOut.value = Number(effective.pitch).toFixed(2);
  els.highlight.checked = effective.highlight;
  els.jumpOnSelect.checked = effective.jumpOnSelect;
  for (const field of PAUSE_FIELDS) pauseEls[field].value = effective[field];
  populateVoices(effective.voiceURI);
}

async function persist() {
  const patch = {
    rate: Number(els.rate.value),
    pitch: Number(els.pitch.value),
    voiceURI: els.voice.value,
    highlight: els.highlight.checked,
    jumpOnSelect: els.jumpOnSelect.checked,
  };
  for (const field of PAUSE_FIELDS) patch[field] = Number(pauseEls[field].value);

  if (els.perSite.checked && hostname) {
    await ReadAloudSettings.saveForSite(hostname, patch);
  } else {
    if (hostname) await ReadAloudSettings.clearSite(hostname);
    await ReadAloudSettings.saveGlobal(patch);
  }
  els.rateOut.value = `${patch.rate.toFixed(2)}x`;
  els.pitchOut.value = patch.pitch.toFixed(2);
}

async function command(type, label) {
  const result = await send({ type });
  if (!result?.ok) {
    setStatus(result?.error || 'Failed');
    return;
  }
  const { playing, paused, index, total } = result.response || {};
  if (total === 0) setStatus('No readable text found on this page.');
  else if (playing && !paused) setStatus(`${label} — segment ${index + 1} of ${total}`);
  else if (paused) setStatus(`Paused — segment ${index + 1} of ${total}`);
  else setStatus('Stopped');
}

document.getElementById('play').addEventListener('click', () => command('play', 'Reading'));
document.getElementById('pause').addEventListener('click', () => command('pause', 'Paused'));
document.getElementById('stop').addEventListener('click', () => command('stop', 'Stopped'));
document.getElementById('next').addEventListener('click', () => command('skipNext', 'Reading'));
document.getElementById('prev').addEventListener('click', () => command('skipPrev', 'Reading'));

document.getElementById('openOptions').addEventListener('click', (event) => {
  event.preventDefault();
  chrome.runtime.openOptionsPage();
});

for (const el of [els.rate, els.pitch, els.voice, els.highlight, els.jumpOnSelect, els.perSite, ...Object.values(pauseEls)]) {
  el.addEventListener('change', persist);
}
els.rate.addEventListener('input', () => {
  els.rateOut.value = `${Number(els.rate.value).toFixed(2)}x`;
});
els.pitch.addEventListener('input', () => {
  els.pitchOut.value = Number(els.pitch.value).toFixed(2);
});

speechSynthesis.addEventListener('voiceschanged', () => populateVoices(els.voice.value));
loadSettings();
