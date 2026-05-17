// History tab: flatpickr, sendHistoryQuery, drawHistChart, histLabel

import { $ } from './utils.js';

function fmtDate(d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }
function getHistPeriod() { return document.querySelector('input[name=histPeriod]:checked').value; }
var histDebounceTimer = null;
function histQueryDebounced() { clearTimeout(histDebounceTimer); histDebounceTimer = setTimeout(sendHistoryQuery, 400); }

var histPicker = flatpickr('#histDate', {
  theme: 'dark', dateFormat: 'Y-m-d', defaultDate: new Date(), disableMobile: true,
  onChange: function () { histQueryDebounced(); }
});

function rebuildPicker() {
  var period = getHistPeriod();
  var current = histPicker.selectedDates[0] || new Date();
  histPicker.destroy();
  var opts = { theme: 'dark', defaultDate: current, disableMobile: true, onChange: function () { histQueryDebounced(); } };
  if (period === 'month') {
    opts.plugins = [new monthSelectPlugin({ shorthand: false, dateFormat: 'Y-m-d', altFormat: 'F Y' })];
  } else if (period === 'year') {
    opts.dateFormat = 'Y';
    opts.altInput = true; opts.altFormat = 'Y';
  } else {
    opts.dateFormat = 'Y-m-d';
  }
  histPicker = flatpickr('#histDate', opts);
}

export function histStep(dir) {
  var d = histPicker.selectedDates[0] || new Date();
  var period = getHistPeriod();
  if (period === 'day') d.setDate(d.getDate() + dir);
  else if (period === 'week') d.setDate(d.getDate() + dir * 7);
  else if (period === 'month') d.setMonth(d.getMonth() + dir);
  else if (period === 'year') d.setFullYear(d.getFullYear() + dir);
  histPicker.setDate(d, false); histQueryDebounced();
}

export function histGoTo(offset) {
  var d = new Date(); d.setDate(d.getDate() + offset);
  histPicker.setDate(d, false); histQueryDebounced();
}

export function initHistoryListeners() {
  document.querySelectorAll('input[name=histPeriod]').forEach(function (r) {
    r.addEventListener('change', function () { rebuildPicker(); histQueryDebounced(); });
  });
}

export function sendHistoryQuery() {
  var sel = histPicker.selectedDates[0];
  var date = sel ? fmtDate(sel) : fmtDate(new Date());
  var period = getHistPeriod();
  $('histTitle').textContent = 'Loading...';
  $('histResult').style.display = 'none';
  fetch('/api/history-query', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ start: date, period: period }) })
    .then(r => r.json()).then(d => {
      $('histTitle').textContent = 'Energy History — ' + d.start + ' (' + d.period + ')';
      if (!d.dataPoints || !d.dataPoints.length) { $('histResult').style.display = 'block'; $('histResult').textContent = 'No data for this period.'; clearHistChart(); return; }
      drawHistChart(d.dataPoints, d.period, d.start);
    }).catch(e => { $('histTitle').textContent = 'Energy History'; $('histResult').style.display = 'block'; $('histResult').textContent = 'Error: ' + e; });
}

export function histLabel(i, period, startStr) {
  var s = new Date(startStr + 'T00:00:00');
  var p = period.toLowerCase();
  var dayNames = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];
  var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  if (p === 'day') { var h = Math.floor(i * 15 / 60), m = (i * 15) % 60; return (h < 10 ? '0' : '') + h + ':' + (m < 10 ? '0' : '') + m; }
  if (p === 'week') { var d = new Date(s); d.setDate(d.getDate() + i); return dayNames[d.getDay() === 0 ? 6 : d.getDay() - 1]; }
  if (p === 'month') { var d = new Date(s); d.setDate(d.getDate() + i); return '' + d.getDate(); }
  if (p === 'year') { var d = new Date(s); d.setMonth(d.getMonth() + i); return monthNames[d.getMonth()]; }
  return '' + i;
}

function clearHistChart() { var c = $('histChart'); if (!c) return; var ctx = c.getContext('2d'); ctx.clearRect(0, 0, c.width, c.height); }

function drawHistChart(pts, period, startStr) {
  var c = $('histChart'); if (!c) return;
  var ctx = c.getContext('2d'), dpr = devicePixelRatio || 1;
  var r = c.parentElement.getBoundingClientRect(), W = r.width, H = 250;
  c.width = W * dpr; c.height = H * dpr; c.style.width = W + 'px'; c.style.height = H + 'px';
  ctx.scale(dpr, dpr); ctx.clearRect(0, 0, W, H);
  var p = { t: 10, b: 30, l: 55, r: 10 }, cW = W - p.l - p.r, cH = H - p.t - p.b;
  var vals = pts.flatMap(d => [d.solar || 0, d.batIn || 0, d.gridIn || 0, d.consumption || 0]);
  var mx = Math.max(1, ...vals) * 1.15;
  // Grid lines
  ctx.strokeStyle = 'rgba(36,40,48,.9)'; ctx.lineWidth = 1;
  ctx.fillStyle = '#6b7280'; ctx.font = '10px DM Mono'; ctx.textAlign = 'right';
  for (var i = 0; i <= 4; i++) { var y = p.t + i / 4 * cH; ctx.beginPath(); ctx.moveTo(p.l, y); ctx.lineTo(W - p.r, y); ctx.stroke(); ctx.fillText((mx * (1 - i / 4) / 1000).toFixed(1) + 'kWh', p.l - 6, y + 4); }
  // Bars
  var n = pts.length, bw = cW / n, gw = bw * 0.8, cols = ['#5CC244', '#4ea8de', '#f05545', '#f0a030'], keys = ['solar', 'batIn', 'gridIn', 'consumption'];
  var sw = gw / 4;
  for (var i = 0; i < n; i++) {
    var x = p.l + i * bw + (bw - gw) / 2;
    for (var k = 0; k < 4; k++) {
      var v = pts[i][keys[k]] || 0;
      var bh = (v / mx) * cH;
      ctx.fillStyle = cols[k];
      ctx.fillRect(x + k * sw, p.t + cH - bh, sw - 1, bh);
    }
    // X label
    if (n <= 31 || (i % Math.ceil(n / 12) === 0)) {
      ctx.fillStyle = '#6b7280'; ctx.font = '9px DM Mono'; ctx.textAlign = 'center';
      ctx.fillText(histLabel(i, period, startStr), x + gw / 2, H - p.b + 14);
    }
  }
}
