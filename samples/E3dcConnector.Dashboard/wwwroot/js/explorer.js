// Explorer tab: buildExpTree, sendCustomTag, renderItemTree

import { $, state } from './utils.js';

var expNsState = {};

export function buildExpTree() {
  if (!state.lastData) return;
  var d = state.lastData;
  var groups = {
    'EMS': { pvWatts: 'EMS_POWER_PV', batteryWatts: 'EMS_POWER_BAT', gridWatts: 'EMS_POWER_GRID', homeWatts: 'EMS_POWER_HOME', soc: 'EMS_BAT_SOC', autarky: 'EMS_AUTARKY', selfConsumption: 'EMS_SELF_CONSUMPTION' },
    'BAT[0]': { batteryVoltage: 'BAT_MODULE_VOLTAGE', batteryCurrent: 'BAT_CURRENT', chargeCycles: 'BAT_CHARGE_CYCLES' },
    'PVI[0]': { pviAcPowerL1: 'PVI_AC_POWER', pviAcVoltageL1: 'PVI_AC_VOLTAGE', pviDcPower: 'PVI_DC_POWER', pviDcVoltage: 'PVI_DC_VOLTAGE', pviDcCurrent: 'PVI_DC_CURRENT', pviFrequency: 'PVI_AC_FREQUENCY' },
    'PM[0]': { pmPowerL1: 'PM_POWER_L1', pmPowerL2: 'PM_POWER_L2', pmPowerL3: 'PM_POWER_L3', pmVoltageL1: 'PM_VOLTAGE_L1', pmVoltageL2: 'PM_VOLTAGE_L2', pmVoltageL3: 'PM_VOLTAGE_L3', pmEnergyL1: 'PM_ENERGY_L1', pmEnergyL2: 'PM_ENERGY_L2', pmEnergyL3: 'PM_ENERGY_L3' },
    'DCDC[0]': { dcdcBatteryCurrent: 'DCDC_I_BAT', dcdcBatteryVoltage: 'DCDC_U_BAT', dcdcBatteryPower: 'DCDC_P_BAT' },
    'EP': { epIsReadyForSwitch: 'EP_IS_READY_FOR_SWITCH', epIsGridConnected: 'EP_IS_GRID_CONNECTED', epIsIslandGrid: 'EP_IS_ISLAND_GRID' },
    'WB[0]': { wbEnergyAll: 'WB_ENERGY_ALL', wbEnergySolar: 'WB_ENERGY_SOLAR', wbStatus: 'WB_STATUS', wbMode: 'WB_MODE', wbPowerL1: 'WB_PM_POWER_L1', wbPowerL2: 'WB_PM_POWER_L2', wbPowerL3: 'WB_PM_POWER_L3' }
  };
  var units = { pvWatts: 'W', batteryWatts: 'W', gridWatts: 'W', homeWatts: 'W', soc: '%', autarky: '%', selfConsumption: '%', batteryVoltage: 'V', batteryCurrent: 'A', chargeCycles: '', pviAcPowerL1: 'W', pviAcVoltageL1: 'V', pviDcPower: 'W', pviDcVoltage: 'V', pviDcCurrent: 'A', pviFrequency: 'Hz', pmPowerL1: 'W', pmPowerL2: 'W', pmPowerL3: 'W', pmVoltageL1: 'V', pmVoltageL2: 'V', pmVoltageL3: 'V', pmEnergyL1: 'Wh', pmEnergyL2: 'Wh', pmEnergyL3: 'Wh', dcdcBatteryCurrent: 'A', dcdcBatteryVoltage: 'V', dcdcBatteryPower: 'W', epIsReadyForSwitch: '', epIsGridConnected: '', epIsIslandGrid: '', wbEnergyAll: 'Wh', wbEnergySolar: 'Wh', wbStatus: '', wbMode: '', wbPowerL1: 'W', wbPowerL2: 'W', wbPowerL3: 'W' };
  var html = '';
  for (var ns in groups) {
    var isOpen = expNsState[ns] !== false;
    html += '<div class="exp-ns-hdr' + (isOpen ? ' open' : '') + '" onclick="toggleExpNs(this,\'' + ns + '\')">' + ns + '</div>';
    html += '<div class="exp-ns-items' + (isOpen ? ' open' : '') + '">';
    var tags = groups[ns];
    for (var key in tags) {
      var v = d[key];
      var fv = v != null ? (typeof v === 'number' ? (Number.isInteger(v) ? v : v.toFixed(2)) : '' + v) : '—';
      var u = units[key] || '';
      html += '<div class="exp-item"><span class="exp-item-tag">' + tags[key] + '</span><span class="exp-item-val">' + fv + (u ? ' ' + u : '') + '</span></div>';
    }
    html += '</div>';
  }
  $('expTree').innerHTML = html;
  $('expTick').textContent = 'updated ' + new Date().toLocaleTimeString();
}

export function toggleExpNs(el, ns) { expNsState[ns] = !el.classList.contains('open'); buildExpTree(); }

export function renderItemTree(items, depth) {
  var html = '';
  for (var i = 0; i < items.length; i++) {
    var it = items[i];
    html += '<div class="exp-item" style="padding-left:' + (depth * 1) + 'rem">';
    html += '<span class="exp-item-tag">' + it.tag + '</span>';
    html += '<span class="exp-item-type">' + it.type + '</span>';
    if (it.value != null) html += '<span class="exp-item-val">' + it.value + '</span>';
    html += '<span class="exp-item-hex">' + it.hex + '</span>';
    html += '</div>';
    if (it.children) html += renderItemTree(it.children, depth + 1);
  }
  return html;
}

export function sendCustomTag() {
  var tag = $('expInput').value.trim(); if (!tag) return;
  $('expResult').style.display = 'block'; $('expResult').innerHTML = '<span style="color:var(--dim)">Sending...</span>';
  fetch('/api/send', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ tags: [tag] }) })
    .then(r => r.json()).then(d => {
      if (d.error) { $('expResult').innerHTML = '<span style="color:var(--red)">' + d.error + '</span>'; return; }
      $('expResult').innerHTML = renderItemTree(d.items || [], 0);
    }).catch(e => { $('expResult').innerHTML = '<span style="color:var(--red)">Error: ' + e + '</span>'; });
}
