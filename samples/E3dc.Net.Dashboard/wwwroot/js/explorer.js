// Explorer tab: live raw RSCP item tree + on-demand fetch per namespace

import { $ } from './utils.js';

var nsState = {};
var onDemandResults = {};
var lastItemKeys = '';

var fetchableNs = {
  'EMS':  { tags: ['EMS_REQ_POWER_PV','EMS_REQ_POWER_BAT','EMS_REQ_POWER_GRID','EMS_REQ_POWER_HOME','EMS_REQ_POWER_ADD','EMS_REQ_BAT_SOC','EMS_REQ_AUTARKY','EMS_REQ_SELF_CONSUMPTION','EMS_REQ_MODE','EMS_REQ_MAX_CHARGE_POWER','EMS_REQ_MAX_DISCHARGE_POWER','EMS_REQ_EMERGENCY_POWER_STATUS'] },
  'BAT':  { tags: ['BAT_REQ_RSOC','BAT_REQ_MODULE_VOLTAGE','BAT_REQ_CURRENT','BAT_REQ_CHARGE_CYCLES','BAT_REQ_STATUS_CODE','BAT_REQ_ERROR_CODE','BAT_REQ_DCB_COUNT'], device: 'BAT', idx: 0 },
  'PVI':  { tags: ['PVI_REQ_ON_GRID','PVI_REQ_STATE','PVI_REQ_AC_POWER','PVI_REQ_AC_VOLTAGE','PVI_REQ_AC_CURRENT','PVI_REQ_AC_FREQUENCY','PVI_REQ_DC_POWER','PVI_REQ_DC_VOLTAGE','PVI_REQ_DC_CURRENT'], device: 'PVI', idx: 0 },
  'PM':   { tags: ['PM_REQ_POWER_L1','PM_REQ_POWER_L2','PM_REQ_POWER_L3','PM_REQ_VOLTAGE_L1','PM_REQ_VOLTAGE_L2','PM_REQ_VOLTAGE_L3','PM_REQ_ENERGY_L1','PM_REQ_ENERGY_L2','PM_REQ_ENERGY_L3'], device: 'PM', idx: 0 },
  'DCDC': { tags: ['DCDC_REQ_I_BAT','DCDC_REQ_U_BAT','DCDC_REQ_P_BAT'], device: 'DCDC', idx: 0 },
  'WB':   { tags: ['WB_REQ_ENERGY_ALL','WB_REQ_ENERGY_SOLAR','WB_REQ_STATUS','WB_REQ_ERROR_CODE','WB_REQ_MODE','WB_REQ_PM_POWER_L1','WB_REQ_PM_POWER_L2','WB_REQ_PM_POWER_L3'], device: 'WB', idx: 0 },
  'INFO': { tags: ['INFO_REQ_SERIAL_NUMBER','INFO_REQ_PRODUCTION_DATE','INFO_REQ_SW_RELEASE','INFO_REQ_IP_ADDRESS','INFO_REQ_SUBNET_MASK','INFO_REQ_GATEWAY','INFO_REQ_DNS','INFO_REQ_TIME','INFO_REQ_TIME_ZONE'] },
  'EP':   { tags: ['EP_REQ_IS_READY_FOR_SWITCH','EP_REQ_IS_GRID_CONNECTED','EP_REQ_IS_ISLAND_GRID'] },
  'HA':   { tags: ['HA_REQ_DATAPOINT_LIST','HA_REQ_ACTUATOR_STATES'] },
  'SYS':  { tags: [], noFetch: true },
  'UM':   { tags: ['UM_REQ_UPDATE_STATUS'] },
};

export function buildExpTree() {
  fetch('/api/raw-items')
    .then(r => r.json())
    .then(items => updateTree(items))
    .catch(() => {});
}

function mergeItems(liveItems) {
  var grouped = {};
  (liveItems || []).forEach(function(item) {
    var ns = getNamespace(item.tag);
    if (!grouped[ns]) grouped[ns] = [];
    grouped[ns].push(item);
  });
  Object.keys(onDemandResults).forEach(function(ns) {
    if (!grouped[ns]) grouped[ns] = [];
    var existing = new Set(grouped[ns].map(function(i) { return i.tag; }));
    onDemandResults[ns].forEach(function(item) {
      if (!existing.has(item.tag)) grouped[ns].push(item);
    });
  });
  return grouped;
}

function collectKeys(items, prefix) {
  var keys = [];
  (items || []).forEach(function(item) {
    keys.push(prefix + item.tag);
    if (item.children) keys = keys.concat(collectKeys(item.children, prefix + item.tag + '/'));
  });
  return keys;
}

function updateTree(liveItems) {
  var grouped = mergeItems(liveItems);

  // Build a key fingerprint to detect structural changes
  var allKeys = [];
  Object.keys(fetchableNs).forEach(function(ns) {
    var items = grouped[ns] || [];
    allKeys.push(ns + ':' + items.length);
    items.forEach(function(i) { allKeys = allKeys.concat(collectKeys([i], '')); });
  });
  var keyStr = allKeys.join('|');

  if (keyStr !== lastItemKeys) {
    lastItemKeys = keyStr;
    fullRender(grouped);
  } else {
    patchValues(grouped);
  }

  $('expTick').textContent = 'updated ' + new Date().toLocaleTimeString();
}

function patchValues(grouped) {
  var tree = $('expTree');
  tree.querySelectorAll('[data-val-key]').forEach(function(el) {
    var key = el.getAttribute('data-val-key');
    var parts = key.split('/');
    var ns = parts[0];
    var path = parts.slice(1);
    var item = findItem(grouped[ns] || [], path);
    if (!item) return;
    var valEl = el.querySelector('.exp-item-val');
    var hexEl = el.querySelector('.exp-item-hex');
    var newVal = item.value !== undefined && item.value !== null ? '' + item.value : '';
    var newHex = item.hex || '';
    if (valEl && valEl.textContent !== newVal) valEl.textContent = newVal;
    if (hexEl && hexEl.textContent !== newHex) hexEl.textContent = newHex;
    var copyBtns = el.querySelectorAll('.exp-copy');
    copyBtns.forEach(function(btn) {
      if (btn.title === 'Copy value') btn.setAttribute('data-copy', newVal);
      if (btn.title === 'Copy hex') btn.setAttribute('data-copy', newHex);
    });
  });
}

function findItem(items, path) {
  if (!path.length || !items) return null;
  var tag = path[0];
  for (var i = 0; i < items.length; i++) {
    if (items[i].tag === tag) {
      if (path.length === 1) return items[i];
      return findItem(items[i].children || [], path.slice(1));
    }
  }
  return null;
}

function fullRender(grouped) {
  var allNs = Object.keys(fetchableNs);
  var html = '';

  allNs.forEach(function(ns) {
    var items = grouped[ns] || [];
    var hasData = items.length > 0;
    var isOpen = nsState[ns] !== false && hasData;
    var isFetching = nsState[ns + '_fetching'];
    var conf = fetchableNs[ns] || {};

    html += '<div class="exp-ns-hdr' + (isOpen ? ' open' : '') + '" onclick="toggleExpNs(this,\'' + ns + '\')">';
    html += '<span>' + ns + '</span>';
    if (hasData) html += '<span class="exp-ns-count">' + items.length + '</span>';
    if (!conf.noFetch && conf.tags && conf.tags.length)
      html += '<button class="exp-fetch-btn' + (isFetching ? ' loading' : '') + '" onclick="event.stopPropagation();fetchNs(\'' + ns + '\')" title="Fetch all ' + ns + ' tags">' + (isFetching ? '...' : '↻') + '</button>';
    html += '</div>';

    html += '<div class="exp-ns-items' + (isOpen ? ' open' : '') + '">';
    if (hasData) {
      items.forEach(function(item) { html += renderItem(item, 0, ns + '/' + item.tag); });
    } else {
      html += '<div style="color:var(--muted);font-size:.65rem;padding:.2rem 0">No polled data — click ↻ to fetch</div>';
    }
    html += '</div>';
  });

  $('expTree').innerHTML = html;
}

function renderItem(item, depth, keyPath) {
  var isContainer = item.type === 'Container' && item.children && item.children.length;
  var isError = item.type === 'Error';
  var indent = depth * 0.75;

  var marks = '';
  for (var d = 1; d <= depth; d++) {
    marks += '<span class="exp-depth-mark" style="left:' + ((d - 1) * 0.75 + 0.15) + 'rem"></span>';
  }
  var html = '<div class="exp-item" data-val-key="' + keyPath + '">';
  html += '<span class="exp-item-tag' + (isError ? ' error' : '') + '" style="padding-left:' + indent + 'rem">' +
    marks + item.tag + '</span>';
  html += '<span class="exp-item-type">' + item.type + '</span>';
  var val = (item.value !== undefined && item.value !== null) ? '' + item.value : '';
  var hex = item.hex || '';
  html += '<span class="exp-item-val">' + val + '</span>';
  html += '<span class="exp-item-hex">' + hex + '</span>';
  html += '<span class="exp-actions">';
  if (val) html += '<button class="exp-copy" data-copy="' + val.replace(/"/g, '&quot;') + '" onclick="expCopy(this)" title="Copy value">val</button>';
  if (hex) html += '<button class="exp-copy" data-copy="' + hex.replace(/"/g, '&quot;') + '" onclick="expCopy(this)" title="Copy hex">hex</button>';
  html += '</span>';
  html += '</div>';

  if (isContainer) {
    item.children.forEach(function(child) {
      html += renderItem(child, depth + 1, keyPath + '/' + child.tag);
    });
  }

  return html;
}

function getNamespace(tagName) {
  var m = tagName.match(/^([A-Z]+)_/);
  return m ? m[1] : 'OTHER';
}

export function toggleExpNs(el, ns) {
  nsState[ns] = !el.classList.contains('open');
  lastItemKeys = '';
  buildExpTree();
}

window.expCopy = function(btn) {
  var text = btn.getAttribute('data-copy');
  if (!text) return;
  var label = btn.textContent;
  navigator.clipboard.writeText(text).then(function() {
    btn.textContent = '✓';
    setTimeout(function() { btn.textContent = label; }, 1000);
  });
};

window.fetchNs = function(ns) {
  var config = fetchableNs[ns];
  if (!config || !config.tags || !config.tags.length) return;

  nsState[ns + '_fetching'] = true;
  lastItemKeys = '';
  buildExpTree();

  var body = { tags: config.tags };
  if (config.device) {
    body.deviceNamespace = config.device;
    body.deviceIndex = config.idx;
  }

  fetch('/api/send', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  })
    .then(r => r.json())
    .then(d => {
      nsState[ns + '_fetching'] = false;
      if (d.items) {
        onDemandResults[ns] = d.items;
        nsState[ns] = true;
      }
      lastItemKeys = '';
      buildExpTree();
    })
    .catch(() => {
      nsState[ns + '_fetching'] = false;
      lastItemKeys = '';
      buildExpTree();
    });
};
