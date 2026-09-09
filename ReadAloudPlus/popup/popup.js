const els = {
  status: document.getElementById('status'),
  voice: document.getElementById('voice'),
  rate: document.getElementById('rate'),
  rateOut: document.getElementById('rateOut'),
  pitch: document.getElementById('pitch'),
  pitchOut: document.getElementById('pitchOut'),
  voiceHint: document.getElementById('voiceHint'),
};

let hostname = '';
let hasOverride = false;

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

async function refreshFromSettings() {
  hostname = await currentHostname();

  const all = await ReadAloudSettings.loadSettings();
  hasOverride = Boolean(all.perSite[hostname]);

  const effective = { ...all.global, ...(all.perSite[hostname] || {}) };
  els.rate.value = effective.rate;
  els.rateOut.value = `${Number(effective.rate).toFixed(2)}x`;
  els.pitch.value = effective.pitch;
  els.pitchOut.value = Number(effective.pitch).toFixed(2);
  populateVoices(effective.voiceURI);
}

async function persist() {
  const patch = {
    rate: Number(els.rate.value),
    pitch: Number(els.pitch.value),
    voiceURI: els.voice.value,
  };

  // If this site already has its own overrides (set via the options page), keep
  // editing those so the quick controls actually affect what's playing here;
  // otherwise a popup edit would silently do nothing on an overridden site.
  if (hasOverride && hostname) {
    await ReadAloudSettings.saveForSite(hostname, patch);
  } else {
    await ReadAloudSettings.saveGlobal(patch);
  }
  els.rateOut.value = `${patch.rate.toFixed(2)}x`;
  els.pitchOut.value = patch.pitch.toFixed(2);

  // Apply voice/speed/pitch to what's currently playing, instead of only on the next Play.
  const result = await send({ type: 'refreshSettings' });
  if (result?.ok && result.response) {
    const { playing, paused, index, total } = result.response;
    if (playing && !paused) setStatus(`Reading — segment ${index + 1} of ${total}`);
  }
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

for (const el of [els.rate, els.pitch, els.voice]) {
  el.addEventListener('change', persist);
}
els.rate.addEventListener('input', () => {
  els.rateOut.value = `${Number(els.rate.value).toFixed(2)}x`;
});
els.pitch.addEventListener('input', () => {
  els.pitchOut.value = Number(els.pitch.value).toFixed(2);
});

speechSynthesis.addEventListener('voiceschanged', () => populateVoices(els.voice.value));
refreshFromSettings();
