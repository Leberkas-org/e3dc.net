// Dashboard tab: update function, drawChart, pipe animations

import { $, circ, state, fa, fv, epBool, setPipe, setVPipe, setMPipe, bdg } from './utils.js';
import { buildExpTree } from './explorer.js';

export function update(d) {
  state.lastData = d;
  $('dot').classList.add('on');
  $('connTxt').textContent = new Date(d.timestamp).toLocaleTimeString();

  var soc = d.soc ?? 0;
  var batC = d.batteryWatts > 50, batD = d.batteryWatts < -50;
  var gridI = d.gridWatts > 50, gridF = d.gridWatts < -50;

  // Schematic nodes
  $('vPv').innerHTML = fa(d.pvWatts) + '<span class="u">W</span>';
  $('vPv').className = 'node-val ' + (d.pvWatts > 50 ? 'vg' : 'vd');
  $('iPv').className = 'node-icon' + (d.pvWatts > 50 ? ' glow-g' : '');

  $('vHome').innerHTML = fa(d.homeWatts) + '<span class="u">W</span>';
  $('vHome').className = 'node-val vt';
  $('iHome').className = 'node-icon' + (d.homeWatts > 50 ? ' glow-g' : '');

  $('vGrid').innerHTML = fa(d.gridWatts) + '<span class="u">W</span>';
  $('vGrid').className = 'node-val ' + (gridI ? 'vr' : gridF ? 'vg' : 'vd');
  $('sGrid').textContent = gridI ? 'Import' : gridF ? 'Feed-in' : 'Idle';
  $('sGrid').className = 'node-state ' + (gridI ? 'vr' : gridF ? 'vg' : 'vd');
  $('iGrid').className = 'node-icon' + (gridI ? ' glow-r' : gridF ? ' glow-g' : '');

  // Battery radial
  var off = circ - (soc / 100) * circ;
  var ring = $('batRing');
  ring.style.strokeDashoffset = off;
  ring.classList.remove('low', 'mid');
  if (soc < 20) ring.classList.add('low'); else if (soc < 45) ring.classList.add('mid');
  $('batSoc').textContent = soc.toFixed(1) + '%';
  $('batPow').innerHTML = fa(d.batteryWatts) + '<span class="u">W</span>';
  $('batPow').className = 'bat-power ' + (batC ? 'vg' : batD ? 'vr' : 'vd');
  $('batSt').textContent = batC ? 'Charging' : batD ? 'Discharging' : 'Idle';
  $('batSt').className = 'bat-state ' + (batC ? 'vg' : batD ? 'vr' : 'vd');

  // Pipes (desktop)
  setPipe('lnPB', 'arPB', d.pvWatts, 'r', 'l');
  setPipe('lnBH', 'arBH', batD ? -1 : d.homeWatts > 50 ? 1 : 0, 'r', 'l');
  setVPipe('lnGB', 'arGB', d.gridWatts);
  // Pipes (mobile vertical)
  setMPipe('mlnPB', 'marPB', d.pvWatts);
  setMPipe('mlnBH', 'marBH', batD ? -1 : d.homeWatts > 50 ? 1 : 0);
  setMPipe('mlnGB', 'marGB', d.gridWatts);

  // Tiles
  $('tvPv').innerHTML = fa(d.pvWatts) + '<span class="u">W</span>';
  $('tvPv').className = 'tile-v ' + (d.pvWatts > 50 ? 'vg' : 'vd');
  bdg('bPv', d.pvWatts, 'PRODUCING', 'IDLE', 'g', 'd');
  $('tPv').className = 'tile' + (d.pvWatts > 50 ? ' ag' : '');

  $('tvBat').innerHTML = fa(d.batteryWatts) + '<span class="u">W</span>';
  $('tvBat').className = 'tile-v ' + (batC ? 'vg' : batD ? 'vr' : 'vd');
  bdg('bBat', d.batteryWatts, 'CHARGING', 'DISCHARGING', 'g', 'r');
  $('tBat').className = 'tile' + (batC ? ' ag' : batD ? ' ar' : '');

  $('tvGrid').innerHTML = fa(d.gridWatts) + '<span class="u">W</span>';
  $('tvGrid').className = 'tile-v ' + (gridI ? 'vr' : gridF ? 'vg' : 'vd');
  bdg('bGrid', d.gridWatts, 'IMPORT', 'FEED-IN', 'r', 'g');
  $('tGrid').className = 'tile' + (gridI ? ' ar' : gridF ? ' ag' : '');

  $('tvHome').innerHTML = fa(d.homeWatts) + '<span class="u">W</span>';
  $('tvHome').className = 'tile-v vt';
  bdg('bHome', d.homeWatts, 'CONSUMING', 'IDLE', 'r', 'd');
  $('tHome').className = 'tile' + (d.homeWatts > 50 ? ' ar' : '');

  // Strip
  $('dSoc').innerHTML = soc.toFixed(1) + ' <span class="u">%</span>';
  $('dVolt').innerHTML = (d.batteryVoltage ?? 0).toFixed(1) + ' <span class="u">V</span>';
  $('dCurr').innerHTML = (d.batteryCurrent ?? 0).toFixed(1) + ' <span class="u">A</span>';
  $('dAut').innerHTML = (d.autarky ?? 0).toFixed(1) + ' <span class="u">%</span>';
  $('dSelf').innerHTML = (d.selfConsumption ?? 0).toFixed(1) + ' <span class="u">%</span>';

  // PVI
  $('pAcP').innerHTML = fv(d.pviAcPowerL1, 0) + ' <span class="u">W</span>';
  $('pAcV').innerHTML = fv(d.pviAcVoltageL1, 1) + ' <span class="u">V</span>';
  $('pDcP').innerHTML = fv(d.pviDcPower, 0) + ' <span class="u">W</span>';
  $('pDcV').innerHTML = fv(d.pviDcVoltage, 1) + ' <span class="u">V</span>';
  $('pDcI').innerHTML = fv(d.pviDcCurrent, 2) + ' <span class="u">A</span>';
  $('pFreq').innerHTML = fv(d.pviFrequency, 2) + ' <span class="u">Hz</span>';
  // PM
  $('mPL1').innerHTML = fv(d.pmPowerL1, 0) + ' <span class="u">W</span>';
  $('mPL2').innerHTML = fv(d.pmPowerL2, 0) + ' <span class="u">W</span>';
  $('mPL3').innerHTML = fv(d.pmPowerL3, 0) + ' <span class="u">W</span>';
  $('mVL1').innerHTML = fv(d.pmVoltageL1, 1) + ' <span class="u">V</span>';
  $('mVL2').innerHTML = fv(d.pmVoltageL2, 1) + ' <span class="u">V</span>';
  $('mVL3').innerHTML = fv(d.pmVoltageL3, 1) + ' <span class="u">V</span>';
  $('mEL1').innerHTML = fv(d.pmEnergyL1, 1) + ' <span class="u">kWh</span>';
  $('mEL2').innerHTML = fv(d.pmEnergyL2, 1) + ' <span class="u">kWh</span>';
  $('mEL3').innerHTML = fv(d.pmEnergyL3, 1) + ' <span class="u">kWh</span>';

  // DCDC
  $('dcI').innerHTML = fv(d.dcdcBatteryCurrent, 2) + ' <span class="u">A</span>';
  $('dcV').innerHTML = fv(d.dcdcBatteryVoltage, 1) + ' <span class="u">V</span>';
  $('dcP').innerHTML = fv(d.dcdcBatteryPower, 0) + ' <span class="u">W</span>';

  // EP
  epBool('epReady', d.epIsReadyForSwitch);
  epBool('epGrid', d.epIsGridConnected);
  epBool('epIsland', d.epIsIslandGrid);

  // WB
  $('wbEAll').innerHTML = fv(d.wbEnergyAll != null ? d.wbEnergyAll / 1000 : null, 1) + ' <span class="u">kWh</span>';
  $('wbESol').innerHTML = fv(d.wbEnergySolar != null ? d.wbEnergySolar / 1000 : null, 1) + ' <span class="u">kWh</span>';
  $('wbStat').innerHTML = fv(d.wbStatus, 0);
  $('wbMode').innerHTML = fv(d.wbMode, 0);
  $('wbPL1').innerHTML = fv(d.wbPowerL1, 0) + ' <span class="u">W</span>';
  $('wbPL2').innerHTML = fv(d.wbPowerL2, 0) + ' <span class="u">W</span>';
  $('wbPL3').innerHTML = fv(d.wbPowerL3, 0) + ' <span class="u">W</span>';

  state.hist.push({ t: Date.now(), pv: d.pvWatts, bat: d.batteryWatts, grid: d.gridWatts, home: d.homeWatts });
  while (state.hist.length > state.MAX_H) state.hist.shift();
  drawChart();
  if ($('tab-explorer').classList.contains('active')) buildExpTree();
}

export function drawChart() {
  var c = $('chart'); if (!c) return;
  var ctx = c.getContext('2d'), dpr = devicePixelRatio || 1;
  var r = c.parentElement.getBoundingClientRect(), W = r.width, H = Math.max(80, r.height);
  c.width = W * dpr; c.height = H * dpr; c.style.width = W + 'px'; c.style.height = H + 'px';
  ctx.scale(dpr, dpr); ctx.clearRect(0, 0, W, H);
  if (state.hist.length < 2) return;
  var all = state.hist.flatMap(d => [d.pv, Math.abs(d.bat), Math.abs(d.grid), d.home]);
  var mx = Math.max(200, ...all) * 1.1;
  var p = { t: 10, b: 20, l: 48, r: 10 }, cW = W - p.l - p.r, cH = H - p.t - p.b;
  ctx.strokeStyle = 'rgba(36,40,48,.9)'; ctx.lineWidth = 1;
  ctx.fillStyle = '#6b7280'; ctx.font = '10px DM Mono'; ctx.textAlign = 'right';
  for (var i = 0; i <= 4; i++) { var y = p.t + i / 4 * cH; ctx.beginPath(); ctx.moveTo(p.l, y); ctx.lineTo(W - p.r, y); ctx.stroke(); ctx.fillText(Math.round(mx * (1 - i / 4)), p.l - 8, y + 4); }
  function ln(k, col, abs) { ctx.beginPath(); ctx.strokeStyle = col; ctx.lineWidth = 1.5; ctx.lineJoin = 'round'; for (var i = 0; i < state.hist.length; i++) { var x = p.l + i / (state.hist.length - 1) * cW, v = abs ? Math.abs(state.hist[i][k]) : state.hist[i][k], y = p.t + cH - (v / mx) * cH; i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y); } ctx.stroke(); }
  ln('pv', '#5CC244', false); ln('bat', '#4ea8de', true); ln('grid', '#f05545', true); ln('home', '#f0a030', false);
}

export function sysConfirm(action, tag) {
  if (!confirm('Are you sure you want to ' + action + '? This will affect the E3DC system.')) return;
  $('sysResult').style.display = 'block';
  $('sysResult').textContent = 'Sending ' + action + ' command...';
  fetch('/api/send', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ tags: [tag] }) })
    .then(r => r.json()).then(d => {
      $('sysResult').textContent = d.error || 'Command sent. ' + action + ' in progress.';
    }).catch(e => { $('sysResult').textContent = 'Error: ' + e; });
}

export function sysReboot() { sysConfirm('REBOOT', 'SYS_REQ_SYSTEM_REBOOT'); }
export function sysRestart() { sysConfirm('RESTART APPLICATION', 'SYS_REQ_RESTART_APPLICATION'); }
