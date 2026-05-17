// Request Builder tab: loadTags, rbAdd, rbRemove, rbSend

import { $ } from './utils.js';

var rbTagData = null;
var rbSelected = [];
var indexedNs = ['BAT', 'PVI', 'PM', 'WB'];

export function isTagDataLoaded() { return rbTagData !== null; }

export function loadTags() {
  fetch('/api/tags').then(r => r.json()).then(d => {
    rbTagData = d;
    var ns = $('rbNs'); ns.innerHTML = '<option value="">Namespace...</option>';
    Object.keys(d).sort().forEach(k => { var o = document.createElement('option'); o.value = k; o.textContent = k + ' (' + d[k].length + ')'; ns.appendChild(o); });
  }).catch(() => { });
}

export function initBuilderListeners() {
  $('rbNs').onchange = function () {
    var ns = this.value, sel = $('rbTag');
    sel.innerHTML = '<option value="">Tag...</option>';
    $('rbIdx').style.display = indexedNs.includes(ns) ? 'inline-block' : 'none';
    if (ns && rbTagData && rbTagData[ns]) {
      rbTagData[ns].forEach(t => { var o = document.createElement('option'); o.value = t.name; o.textContent = t.name; sel.appendChild(o); });
    }
  };
}

export function rbAdd() {
  var tag = $('rbTag').value; if (!tag || rbSelected.includes(tag)) return;
  rbSelected.push(tag); rbRenderChips();
}

export function rbRemove(tag) { rbSelected = rbSelected.filter(t => t !== tag); rbRenderChips(); }

function rbRenderChips() {
  $('rbChips').innerHTML = rbSelected.map(t => '<div class="rb-chip">' + t + '<button onclick="rbRemove(\'' + t + '\')">&#215;</button></div>').join('');
}

export function rbSend() {
  if (!rbSelected.length) return;
  var ns = $('rbNs').value, idx = $('rbIdx').value;
  var body = { tags: rbSelected };
  if (indexedNs.includes(ns)) { body.deviceNamespace = ns; body.deviceIndex = parseInt(idx) || 0; }
  $('rbResult').textContent = 'Sending...';
  fetch('/api/send', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) })
    .then(r => r.text()).then(t => { $('rbResult').textContent = t; }).catch(e => { $('rbResult').textContent = 'Error: ' + e; });
}
