// Main entry point: imports all modules, sets up SSE, tab switching, init

import { $, state } from './utils.js';
import { update, drawChart, sysReboot, sysRestart } from './dashboard.js';
import { sendHistoryQuery, histStep, histGoTo, initHistoryListeners } from './history.js';
import { buildExpTree, toggleExpNs, sendCustomTag } from './explorer.js';
import { loadTags, rbAdd, rbRemove, rbSend, isTagDataLoaded, initBuilderListeners } from './builder.js';

// Register onclick handlers on window (called from HTML onclick attributes)
window.sendHistoryQuery = sendHistoryQuery;
window.histStep = histStep;
window.histGoTo = histGoTo;
window.sendCustomTag = sendCustomTag;
window.toggleExpNs = toggleExpNs;
window.rbAdd = rbAdd;
window.rbRemove = rbRemove;
window.rbSend = rbSend;
window.sysReboot = sysReboot;
window.sysRestart = sysRestart;

// Tab switching
document.querySelectorAll('.tab-btn').forEach(btn => {
  btn.onclick = () => {
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');
    $('tab-' + btn.dataset.tab).classList.add('active');
    if (btn.dataset.tab === 'dashboard') setTimeout(drawChart, 50);
    if (btn.dataset.tab === 'explorer') buildExpTree();
    if (btn.dataset.tab === 'builder' && !isTagDataLoaded()) loadTags();
  };
});

// History tab listeners
initHistoryListeners();

// Builder tab listeners
initBuilderListeners();

// System info
fetch('/api/info').then(r => r.json()).then(d => {
  if (d.serialNumber) $('sysInfo').textContent = 'SN: ' + d.serialNumber + ' · ' + d.swRelease + ' · ' + d.ipAddress;
}).catch(() => { });

// Init: load history and start SSE
fetch('/api/history').then(r => r.json()).then(d => {
  d.forEach(x => state.hist.push({ t: new Date(x.timestamp).getTime(), pv: x.pvWatts, bat: x.batteryWatts, grid: x.gridWatts, home: x.homeWatts }));
  while (state.hist.length > state.MAX_H) state.hist.shift();
  if (state.hist.length > 1) drawChart();
}).catch(() => { });

var es = new EventSource('/api/stream');
es.onmessage = function (e) { update(JSON.parse(e.data)); };
es.onerror = function () { $('dot').classList.remove('on'); $('connTxt').textContent = 'Reconnecting...'; };
addEventListener('resize', drawChart);
