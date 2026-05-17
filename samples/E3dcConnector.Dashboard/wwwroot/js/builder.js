// Request Builder – three-panel RSCP protocol workbench

import { $ } from './utils.js';

var tagDb = null;          // rscp-tags.json loaded once
var selected = [];         // [{ tag, ns }]
var deviceIndices = {};    // { BAT: 0, PVI: 0, ... }

// Which namespaces need device-index containers
var indexedNs = {};        // populated from tagDb.namespaces

export function isTagDataLoaded() { return tagDb !== null; }

// ── Init ───────────────────────────────────────────────────────────

export function loadTags() {
  fetch('/js/rscp-tags.json').then(r => r.json()).then(d => {
    tagDb = d;
    // Build indexed namespace map
    Object.keys(d.namespaces).forEach(ns => {
      if (d.namespaces[ns].indexed) {
        indexedNs[ns] = true;
        deviceIndices[ns] = d.namespaces[ns].defaultIndex || 0;
      }
    });
    renderTree();
  }).catch(() => { });
}

export function initBuilderListeners() {
  // Search
  var debounce = null;
  document.addEventListener('input', function (e) {
    if (e.target.id !== 'wbSearch') return;
    clearTimeout(debounce);
    debounce = setTimeout(function () { renderTree(e.target.value); }, 120);
  });

  // Send / Clear
  document.addEventListener('click', function (e) {
    if (e.target.id === 'wbSend') sendRequest();
    if (e.target.id === 'wbClear') clearRequest();
  });
}

// ── Tag Browser (left panel) ───────────────────────────────────────

function getNamespace(tagName) {
  // Extract namespace prefix: everything before the first _REQ_ or first _
  var m = tagName.match(/^([A-Z]+)_/);
  return m ? m[1] : '';
}

function isRequestTag(tagInfo) {
  return tagInfo.type === 'read' || tagInfo.type === 'write';
}

function renderTree(filter) {
  if (!tagDb) return;
  var el = $('wbTree');
  filter = (filter || '').toLowerCase();

  // Group request tags by namespace
  var groups = {};
  Object.keys(tagDb.tags).forEach(function (name) {
    var info = tagDb.tags[name];
    if (!isRequestTag(info)) return;
    var ns = getNamespace(name);
    if (!ns) return;
    if (filter && name.toLowerCase().indexOf(filter) === -1 && (info.desc || '').toLowerCase().indexOf(filter) === -1) return;
    if (!groups[ns]) groups[ns] = [];
    groups[ns].push({ name: name, info: info });
  });

  var html = '';
  var nsOrder = Object.keys(tagDb.namespaces);
  nsOrder.forEach(function (ns) {
    var tags = groups[ns];
    if (!tags || !tags.length) return;
    var nsInfo = tagDb.namespaces[ns];
    var isOpen = filter ? ' open' : '';
    html += '<div class="wb-ns-hdr' + isOpen + '" onclick="wbToggleNs(this)">' +
      ns + ' <span style="color:var(--muted);font-size:.6rem;font-weight:400">' + (nsInfo.name || '') + '</span>' +
      '<span class="wb-ns-count">' + tags.length + '</span></div>';
    html += '<div class="wb-ns-items' + isOpen + '">';
    tags.forEach(function (t) {
      var badge = t.info.type === 'read' ? 'read' : 'write';
      html += '<div class="wb-tag" data-tag="' + t.name + '" onclick="wbSelectTag(\'' + t.name + '\')" onmouseenter="wbShowDetail(\'' + t.name + '\')">' +
        '<span class="wb-tag-name">' + t.name + '</span>' +
        '<span class="wb-tag-badge ' + badge + '">' + t.info.type + '</span></div>';
    });
    html += '</div>';
  });

  if (!html) html = '<div class="wb-empty">No matching tags</div>';
  el.innerHTML = html;
}

// Toggle namespace in browser
window.wbToggleNs = function (el) {
  el.classList.toggle('open');
  var items = el.nextElementSibling;
  if (items) items.classList.toggle('open');
};

// Show tag detail at bottom of browser panel
window.wbShowDetail = function (tagName) {
  var info = tagDb.tags[tagName];
  if (!info) return;
  var el = $('wbDetail');
  var rows = '<div class="wb-detail-name">' + tagName + '</div>';
  rows += '<div class="wb-detail-row"><span class="wb-detail-label">Desc</span><span class="wb-detail-value">' + (info.desc || '--') + '</span></div>';
  if (info.dataType) rows += '<div class="wb-detail-row"><span class="wb-detail-label">Type</span><span class="wb-detail-value">' + info.dataType + '</span></div>';
  if (info.unit) rows += '<div class="wb-detail-row"><span class="wb-detail-label">Unit</span><span class="wb-detail-value">' + info.unit + '</span></div>';
  if (info.resp) rows += '<div class="wb-detail-row"><span class="wb-detail-label">Resp</span><span class="wb-detail-value">' + info.resp + '</span></div>';
  el.innerHTML = rows;
  el.classList.add('visible');
};

// Click a tag to add it to the composer
window.wbSelectTag = function (tagName) {
  var ns = getNamespace(tagName);
  if (selected.some(function (s) { return s.tag === tagName; })) return;
  selected.push({ tag: tagName, ns: ns });
  renderComposer();
  window.wbShowDetail(tagName);
};

// ── Request Composer (center panel) ─────────────────────────────────

function renderComposer() {
  var el = $('wbRequest');
  $('wbSend').disabled = selected.length === 0;

  if (!selected.length) {
    el.innerHTML = '<div class="wb-empty">Click tags in the browser to compose a request</div>';
    return;
  }

  // Separate flat vs indexed tags
  var flat = [];
  var indexed = {};   // { ns: [tags] }
  selected.forEach(function (s) {
    if (indexedNs[s.ns]) {
      if (!indexed[s.ns]) indexed[s.ns] = [];
      indexed[s.ns].push(s.tag);
    } else {
      flat.push(s);
    }
  });

  var html = '';

  // Flat tags
  if (flat.length) {
    html += '<div class="wb-group">';
    html += '<div class="wb-group-hdr">Flat Tags</div>';
    flat.forEach(function (s) {
      html += renderChip(s.tag);
    });
    html += '</div>';
  }

  // Indexed device tags
  Object.keys(indexed).forEach(function (ns) {
    var idx = deviceIndices[ns] || 0;
    html += '<div class="wb-group">';
    html += '<div class="wb-group-hdr">Device: ' + ns +
      ' [<input class="wb-idx-input" type="number" min="0" value="' + idx + '" onchange="wbSetIndex(\'' + ns + '\',this.value)">]</div>';
    indexed[ns].forEach(function (tag) {
      html += renderChip(tag);
    });
    html += '</div>';
  });

  el.innerHTML = html;
}

function renderChip(tag) {
  return '<span class="wb-chip">' + tag +
    '<button onclick="wbRemoveTag(\'' + tag + '\')" title="Remove">&times;</button></span>';
}

window.wbRemoveTag = function (tag) {
  selected = selected.filter(function (s) { return s.tag !== tag; });
  renderComposer();
};

window.wbSetIndex = function (ns, val) {
  deviceIndices[ns] = parseInt(val) || 0;
};

function clearRequest() {
  selected = [];
  renderComposer();
}

// ── Send Request ────────────────────────────────────────────────────

function sendRequest() {
  if (!selected.length) return;

  var resultEl = $('wbResult');
  resultEl.innerHTML = '<div class="wb-sending">Sending...</div>';
  $('wbSend').disabled = true;

  // Separate flat vs indexed
  var flat = [];
  var indexed = {};
  selected.forEach(function (s) {
    if (indexedNs[s.ns]) {
      if (!indexed[s.ns]) indexed[s.ns] = [];
      indexed[s.ns].push(s.tag);
    } else {
      flat.push(s.tag);
    }
  });

  var requests = [];

  // Flat request (if any)
  if (flat.length) {
    requests.push(
      fetch('/api/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tags: flat })
      }).then(function (r) { return r.json(); })
    );
  }

  // One request per indexed namespace
  Object.keys(indexed).forEach(function (ns) {
    requests.push(
      fetch('/api/send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          tags: indexed[ns],
          deviceNamespace: ns,
          deviceIndex: deviceIndices[ns] || 0
        })
      }).then(function (r) { return r.json(); })
    );
  });

  Promise.all(requests)
    .then(function (results) {
      // Merge all items
      var allItems = [];
      results.forEach(function (r) {
        if (r && r.items) allItems = allItems.concat(r.items);
      });
      renderResponse(allItems);
    })
    .catch(function (err) {
      resultEl.innerHTML = '<div class="wb-resp-error">Error: ' + err.message + '</div>';
    })
    .finally(function () {
      $('wbSend').disabled = selected.length === 0;
    });
}

// ── Response Viewer (right panel) ───────────────────────────────────

function renderResponse(items) {
  var el = $('wbResult');
  if (!items || !items.length) {
    el.innerHTML = '<div class="wb-empty">No response items</div>';
    return;
  }
  el.innerHTML = items.map(function (item) { return renderResponseItem(item, 0); }).join('');
}

function renderResponseItem(item, depth) {
  var id = 'resp_' + Math.random().toString(36).substr(2, 8);
  var isContainer = item.type === 'Container' && item.children && item.children.length;
  var isError = item.type === 'Error' || (item.tag && item.tag.indexOf('ERROR') !== -1);

  var tagClass = isError ? 'wb-resp-error' : 'wb-resp-tag';
  var valDisplay = item.value !== undefined && item.value !== null ? item.value : '';
  var hexDisplay = item.hex || '';

  var html = '<div class="wb-resp-item" style="margin-left:' + (depth * 0) + 'px">';

  if (isContainer) {
    html += '<div class="' + tagClass + '" onclick="wbToggleResp(\'' + id + '\',this)">' +
      item.tag + ' <span class="wb-resp-type">' + item.type + '</span></div>';
    html += '<div class="wb-resp-detail" id="' + id + '">';
    html += '<div class="wb-resp-children">';
    item.children.forEach(function (child) {
      html += renderResponseItem(child, depth + 1);
    });
    html += '</div></div>';
  } else {
    html += '<div class="' + tagClass + '" onclick="wbToggleResp(\'' + id + '\',this)">' +
      item.tag +
      ' <span class="wb-resp-type">' + (item.type || '') + '</span>' +
      '<span class="wb-resp-val">' + valDisplay + '</span>' +
      '</div>';
    html += '<div class="wb-resp-detail" id="' + id + '">';
    if (hexDisplay) html += '<div style="padding:.2rem 0"><span class="wb-resp-hex">' + hexDisplay + '</span></div>';
    html += '</div>';
  }

  html += '</div>';
  return html;
}

window.wbToggleResp = function (id, hdr) {
  var el = document.getElementById(id);
  if (!el) return;
  hdr.classList.toggle('open');
  el.classList.toggle('open');
};

// ── Exported stubs (for app.js compat) ──────────────────────────────

export function rbAdd() { }
export function rbRemove() { }
export function rbSend() { }
