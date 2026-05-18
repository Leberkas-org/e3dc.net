// Shared helpers and state for all dashboard modules

export const $ = id => document.getElementById(id);
export const circ = 2 * Math.PI * 68;

export const state = {
  lastData: null,
  hist: [],
  MAX_H: 900
};

export function fv(v, d) {
  return v != null ? (typeof v === 'number' ? v.toFixed(d ?? 1) : '--') : '--';
}

export function epBool(id, v) {
  var e = $(id);
  if (v == null) { e.textContent = '--'; e.className = 'scell-v'; }
  else { e.textContent = v ? 'Yes' : 'No'; e.className = 'scell-v ' + (v ? 'vg' : 'vr'); }
}

export function fa(w) {
  var a = Math.abs(w);
  return a >= 10000 ? (a / 1000).toFixed(0) + 'k' : a >= 1000 ? (a / 1000).toFixed(1) + 'k' : a.toString();
}

export function setPipe(lnId, arId, watts, dirPos, dirNeg) {
  var ln = $(lnId), ar = $(arId), a = Math.abs(watts) > 50, pos = watts > 50;
  ln.className.baseVal = 'pipe-ln' + (a ? (pos ? ' on a-' + dirPos : ' on-r a-' + dirNeg) : '');
  ar.className.baseVal = 'pipe-ar' + (a ? (pos ? ' on' : ' on-r') : '');
}

export function setVPipe(lnId, arId, watts) {
  var ln = $(lnId), ar = $(arId), a = Math.abs(watts) > 50, imp = watts > 50;
  if (!a) { ln.className.baseVal = 'pipe-ln'; ar.className.baseVal = 'pipe-ar'; ar.setAttribute('points', '5,38 10,48 15,38'); return; }
  if (imp) {
    ln.className.baseVal = 'pipe-ln on-r'; ar.className.baseVal = 'pipe-ar on-r'; ar.setAttribute('points', '5,10 10,0 15,10');
  } else {
    ln.className.baseVal = 'pipe-ln on'; ar.className.baseVal = 'pipe-ar on'; ar.setAttribute('points', '5,38 10,48 15,38');
  }
}

export function setMPipe(lnId, arId, watts) {
  var ln = $(lnId), ar = $(arId); if (!ln) return;
  var a = Math.abs(watts) > 50, pos = watts > 50;
  if (!a) { ln.className.baseVal = 'pipe-ln'; ar.className.baseVal = 'pipe-ar'; return; }
  if (pos) { ln.className.baseVal = 'pipe-ln on'; ar.className.baseVal = 'pipe-ar on'; }
  else { ln.className.baseVal = 'pipe-ln on-r'; ar.className.baseVal = 'pipe-ar on-r'; }
}

export function bdg(id, w, pt, nt, pc, nc) {
  var e = $(id);
  if (w > 50) { e.textContent = pt; e.className = 'badge ' + pc; }
  else if (w < -50) { e.textContent = nt; e.className = 'badge ' + nc; }
  else { e.textContent = 'IDLE'; e.className = 'badge d'; }
}
