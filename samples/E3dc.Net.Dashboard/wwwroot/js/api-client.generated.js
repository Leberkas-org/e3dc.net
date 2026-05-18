export class DashboardClient {
    constructor(baseUrl, http) {
        this.jsonParseReviver = undefined;
        this.http = http ? http : window;
        this.baseUrl = baseUrl ?? "";
    }
    getHistory() {
        let url_ = this.baseUrl + "/api/history";
        url_ = url_.replace(/[?&]$/, "");
        let options_ = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processGetHistory(_response);
        });
    }
    processGetHistory(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                if (Array.isArray(resultData200)) {
                    result200 = [];
                    for (let item of resultData200)
                        result200.push(DashboardSnapshot.fromJS(item));
                }
                else {
                    result200 = null;
                }
                return result200;
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
    getDeviceInfo() {
        let url_ = this.baseUrl + "/api/info";
        url_ = url_.replace(/[?&]$/, "");
        let options_ = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processGetDeviceInfo(_response);
        });
    }
    processGetDeviceInfo(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = DeviceInfoResponse.fromJS(resultData200);
                return result200;
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
}
export class RscpClient {
    constructor(baseUrl, http) {
        this.jsonParseReviver = undefined;
        this.http = http ? http : window;
        this.baseUrl = baseUrl ?? "";
    }
    getTags() {
        let url_ = this.baseUrl + "/api/tags";
        url_ = url_.replace(/[?&]$/, "");
        let options_ = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processGetTags(_response);
        });
    }
    processGetTags(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                if (resultData200) {
                    result200 = {};
                    for (let key in resultData200) {
                        if (resultData200.hasOwnProperty(key))
                            result200[key] = resultData200[key] ? resultData200[key].map((i) => TagEntry.fromJS(i)) : [];
                    }
                }
                else {
                    result200 = null;
                }
                return result200;
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
    sendRscpRequest(body) {
        let url_ = this.baseUrl + "/api/send";
        url_ = url_.replace(/[?&]$/, "");
        const content_ = JSON.stringify(body);
        let options_ = {
            body: content_,
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processSendRscpRequest(_response);
        });
    }
    processSendRscpRequest(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = SendResponse.fromJS(resultData200);
                return result200;
            });
        }
        else if (status === 400) {
            return response.text().then((_responseText) => {
                let result400 = null;
                let resultData400 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result400 = ErrorResponse.fromJS(resultData400);
                return throwException("Invalid request", status, _responseText, _headers, result400);
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
    queryHistory(body) {
        let url_ = this.baseUrl + "/api/history-query";
        url_ = url_.replace(/[?&]$/, "");
        const content_ = JSON.stringify(body);
        let options_ = {
            body: content_,
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processQueryHistory(_response);
        });
    }
    processQueryHistory(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = HistoryQueryResponse.fromJS(resultData200);
                return result200;
            });
        }
        else if (status === 400) {
            return response.text().then((_responseText) => {
                let result400 = null;
                let resultData400 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result400 = ErrorResponse.fromJS(resultData400);
                return throwException("Invalid request", status, _responseText, _headers, result400);
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
}
export class DiagnosticsClient {
    constructor(baseUrl, http) {
        this.jsonParseReviver = undefined;
        this.http = http ? http : window;
        this.baseUrl = baseUrl ?? "";
    }
    getDebugDump() {
        let url_ = this.baseUrl + "/api/debug";
        url_ = url_.replace(/[?&]$/, "");
        let options_ = {
            method: "GET",
            headers: {
                "Accept": "text/plain"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processGetDebugDump(_response);
        });
    }
    processGetDebugDump(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : _responseText;
                result200 = resultData200 !== undefined ? resultData200 : null;
                return result200;
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
    getDiagnostics() {
        let url_ = this.baseUrl + "/api/diag";
        url_ = url_.replace(/[?&]$/, "");
        let options_ = {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        };
        return this.http.fetch(url_, options_).then((_response) => {
            return this.processGetDiagnostics(_response);
        });
    }
    processGetDiagnostics(response) {
        const status = response.status;
        let _headers = {};
        if (response.headers && response.headers.forEach) {
            response.headers.forEach((v, k) => _headers[k] = v);
        }
        ;
        if (status === 200) {
            return response.text().then((_responseText) => {
                let result200 = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = DiagnosticInfo.fromJS(resultData200);
                return result200;
            });
        }
        else if (status !== 200 && status !== 204) {
            return response.text().then((_responseText) => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Promise.resolve(null);
    }
}
export class DashboardSnapshot {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.pvWatts = _data["pvWatts"];
            this.batteryWatts = _data["batteryWatts"];
            this.gridWatts = _data["gridWatts"];
            this.homeWatts = _data["homeWatts"];
            this.soc = _data["soc"];
            this.autarky = _data["autarky"];
            this.selfConsumption = _data["selfConsumption"];
            this.batteryVoltage = _data["batteryVoltage"];
            this.batteryCurrent = _data["batteryCurrent"];
            this.chargeCycles = _data["chargeCycles"];
            this.pviAcPowerL1 = _data["pviAcPowerL1"];
            this.pviAcVoltageL1 = _data["pviAcVoltageL1"];
            this.pviDcPower = _data["pviDcPower"];
            this.pviDcVoltage = _data["pviDcVoltage"];
            this.pviDcCurrent = _data["pviDcCurrent"];
            this.pviFrequency = _data["pviFrequency"];
            this.pmPowerL1 = _data["pmPowerL1"];
            this.pmPowerL2 = _data["pmPowerL2"];
            this.pmPowerL3 = _data["pmPowerL3"];
            this.pmVoltageL1 = _data["pmVoltageL1"];
            this.pmVoltageL2 = _data["pmVoltageL2"];
            this.pmVoltageL3 = _data["pmVoltageL3"];
            this.pmEnergyL1 = _data["pmEnergyL1"];
            this.pmEnergyL2 = _data["pmEnergyL2"];
            this.pmEnergyL3 = _data["pmEnergyL3"];
            this.dcdcBatteryCurrent = _data["dcdcBatteryCurrent"];
            this.dcdcBatteryVoltage = _data["dcdcBatteryVoltage"];
            this.dcdcBatteryPower = _data["dcdcBatteryPower"];
            this.epIsReadyForSwitch = _data["epIsReadyForSwitch"];
            this.epIsGridConnected = _data["epIsGridConnected"];
            this.epIsIslandGrid = _data["epIsIslandGrid"];
            this.wbEnergyAll = _data["wbEnergyAll"];
            this.wbEnergySolar = _data["wbEnergySolar"];
            this.wbStatus = _data["wbStatus"];
            this.wbMode = _data["wbMode"];
            this.wbPowerL1 = _data["wbPowerL1"];
            this.wbPowerL2 = _data["wbPowerL2"];
            this.wbPowerL3 = _data["wbPowerL3"];
            this.timestamp = _data["timestamp"] ? new Date(_data["timestamp"].toString()) : undefined;
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new DashboardSnapshot();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["pvWatts"] = this.pvWatts;
        data["batteryWatts"] = this.batteryWatts;
        data["gridWatts"] = this.gridWatts;
        data["homeWatts"] = this.homeWatts;
        data["soc"] = this.soc;
        data["autarky"] = this.autarky;
        data["selfConsumption"] = this.selfConsumption;
        data["batteryVoltage"] = this.batteryVoltage;
        data["batteryCurrent"] = this.batteryCurrent;
        data["chargeCycles"] = this.chargeCycles;
        data["pviAcPowerL1"] = this.pviAcPowerL1;
        data["pviAcVoltageL1"] = this.pviAcVoltageL1;
        data["pviDcPower"] = this.pviDcPower;
        data["pviDcVoltage"] = this.pviDcVoltage;
        data["pviDcCurrent"] = this.pviDcCurrent;
        data["pviFrequency"] = this.pviFrequency;
        data["pmPowerL1"] = this.pmPowerL1;
        data["pmPowerL2"] = this.pmPowerL2;
        data["pmPowerL3"] = this.pmPowerL3;
        data["pmVoltageL1"] = this.pmVoltageL1;
        data["pmVoltageL2"] = this.pmVoltageL2;
        data["pmVoltageL3"] = this.pmVoltageL3;
        data["pmEnergyL1"] = this.pmEnergyL1;
        data["pmEnergyL2"] = this.pmEnergyL2;
        data["pmEnergyL3"] = this.pmEnergyL3;
        data["dcdcBatteryCurrent"] = this.dcdcBatteryCurrent;
        data["dcdcBatteryVoltage"] = this.dcdcBatteryVoltage;
        data["dcdcBatteryPower"] = this.dcdcBatteryPower;
        data["epIsReadyForSwitch"] = this.epIsReadyForSwitch;
        data["epIsGridConnected"] = this.epIsGridConnected;
        data["epIsIslandGrid"] = this.epIsIslandGrid;
        data["wbEnergyAll"] = this.wbEnergyAll;
        data["wbEnergySolar"] = this.wbEnergySolar;
        data["wbStatus"] = this.wbStatus;
        data["wbMode"] = this.wbMode;
        data["wbPowerL1"] = this.wbPowerL1;
        data["wbPowerL2"] = this.wbPowerL2;
        data["wbPowerL3"] = this.wbPowerL3;
        data["timestamp"] = this.timestamp ? this.timestamp.toISOString() : undefined;
        return data;
    }
}
export class DeviceInfoResponse {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.serialNumber = _data["serialNumber"];
            this.productionDate = _data["productionDate"];
            this.swRelease = _data["swRelease"];
            this.ipAddress = _data["ipAddress"];
            this.subnetMask = _data["subnetMask"];
            this.gateway = _data["gateway"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new DeviceInfoResponse();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["serialNumber"] = this.serialNumber;
        data["productionDate"] = this.productionDate;
        data["swRelease"] = this.swRelease;
        data["ipAddress"] = this.ipAddress;
        data["subnetMask"] = this.subnetMask;
        data["gateway"] = this.gateway;
        return data;
    }
}
export class TagEntry {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.name = _data["name"];
            this.hex = _data["hex"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new TagEntry();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["name"] = this.name;
        data["hex"] = this.hex;
        return data;
    }
}
export class DiagnosticInfo {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.hasSnapshot = _data["hasSnapshot"];
            this.hasEms = _data["hasEms"];
            this.hasBat = _data["hasBat"];
            this.hasPvi = _data["hasPvi"];
            this.hasPm = _data["hasPm"];
            this.hasDcdc = _data["hasDcdc"];
            this.hasEp = _data["hasEp"];
            this.hasWb = _data["hasWb"];
            this.consumerCount = _data["consumerCount"];
            this.lastError = _data["lastError"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new DiagnosticInfo();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["hasSnapshot"] = this.hasSnapshot;
        data["hasEms"] = this.hasEms;
        data["hasBat"] = this.hasBat;
        data["hasPvi"] = this.hasPvi;
        data["hasPm"] = this.hasPm;
        data["hasDcdc"] = this.hasDcdc;
        data["hasEp"] = this.hasEp;
        data["hasWb"] = this.hasWb;
        data["consumerCount"] = this.consumerCount;
        data["lastError"] = this.lastError;
        return data;
    }
}
export class SendRequest {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
        if (!data) {
            this.tags = [];
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            if (Array.isArray(_data["tags"])) {
                this.tags = [];
                for (let item of _data["tags"])
                    this.tags.push(item);
            }
            this.deviceNamespace = _data["deviceNamespace"];
            this.deviceIndex = _data["deviceIndex"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new SendRequest();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        if (Array.isArray(this.tags)) {
            data["tags"] = [];
            for (let item of this.tags)
                data["tags"].push(item);
        }
        data["deviceNamespace"] = this.deviceNamespace;
        data["deviceIndex"] = this.deviceIndex;
        return data;
    }
}
export class SendResponse {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
        if (!data) {
            this.items = [];
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            if (Array.isArray(_data["items"])) {
                this.items = [];
                for (let item of _data["items"])
                    this.items.push(RscpItem.fromJS(item));
            }
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new SendResponse();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        if (Array.isArray(this.items)) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item ? item.toJSON() : undefined);
        }
        return data;
    }
}
export class RscpItem {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.tag = _data["tag"];
            this.type = _data["type"];
            this.hex = _data["hex"];
            this.value = _data["value"];
            if (Array.isArray(_data["children"])) {
                this.children = [];
                for (let item of _data["children"])
                    this.children.push(RscpItem.fromJS(item));
            }
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new RscpItem();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["tag"] = this.tag;
        data["type"] = this.type;
        data["hex"] = this.hex;
        data["value"] = this.value;
        if (Array.isArray(this.children)) {
            data["children"] = [];
            for (let item of this.children)
                data["children"].push(item ? item.toJSON() : undefined);
        }
        return data;
    }
}
export class HistoryQueryRequest {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.start = _data["start"] ? new Date(_data["start"].toString()) : undefined;
            this.period = _data["period"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new HistoryQueryRequest();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["start"] = this.start ? this.start.toISOString() : undefined;
        data["period"] = this.period;
        return data;
    }
}
export class HistoryQueryResponse {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
        if (!data) {
            this.dataPoints = [];
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.period = _data["period"];
            this.start = _data["start"];
            this.summary = _data["summary"] ? HistoryDataPoint.fromJS(_data["summary"]) : undefined;
            if (Array.isArray(_data["dataPoints"])) {
                this.dataPoints = [];
                for (let item of _data["dataPoints"])
                    this.dataPoints.push(HistoryDataPoint.fromJS(item));
            }
            this.count = _data["count"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new HistoryQueryResponse();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["period"] = this.period;
        data["start"] = this.start;
        data["summary"] = this.summary ? this.summary.toJSON() : undefined;
        if (Array.isArray(this.dataPoints)) {
            data["dataPoints"] = [];
            for (let item of this.dataPoints)
                data["dataPoints"].push(item ? item.toJSON() : undefined);
        }
        data["count"] = this.count;
        return data;
    }
}
export class HistoryDataPoint {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.index = _data["index"];
            this.batIn = _data["batIn"];
            this.batOut = _data["batOut"];
            this.gridIn = _data["gridIn"];
            this.gridOut = _data["gridOut"];
            this.solar = _data["solar"];
            this.consumption = _data["consumption"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new HistoryDataPoint();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["index"] = this.index;
        data["batIn"] = this.batIn;
        data["batOut"] = this.batOut;
        data["gridIn"] = this.gridIn;
        data["gridOut"] = this.gridOut;
        data["solar"] = this.solar;
        data["consumption"] = this.consumption;
        return data;
    }
}
export class ErrorResponse {
    constructor(data) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    this[property] = data[property];
            }
        }
    }
    init(_data) {
        if (_data) {
            for (var property in _data) {
                if (_data.hasOwnProperty(property))
                    this[property] = _data[property];
            }
            this.error = _data["error"];
        }
    }
    static fromJS(data) {
        data = typeof data === 'object' ? data : {};
        let result = new ErrorResponse();
        result.init(data);
        return result;
    }
    toJSON(data) {
        data = typeof data === 'object' ? data : {};
        for (var property in this) {
            if (this.hasOwnProperty(property))
                data[property] = this[property];
        }
        data["error"] = this.error;
        return data;
    }
}
export var HistoryQueryRequestPeriod;
(function (HistoryQueryRequestPeriod) {
    HistoryQueryRequestPeriod["Day"] = "day";
    HistoryQueryRequestPeriod["Week"] = "week";
    HistoryQueryRequestPeriod["Month"] = "month";
    HistoryQueryRequestPeriod["Year"] = "year";
})(HistoryQueryRequestPeriod || (HistoryQueryRequestPeriod = {}));
export class SwaggerException extends Error {
    constructor(message, status, response, headers, result) {
        super();
        this.isSwaggerException = true;
        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }
    static isSwaggerException(obj) {
        return obj.isSwaggerException === true;
    }
}
function throwException(message, status, response, headers, result) {
    if (result !== null && result !== undefined)
        throw result;
    else
        throw new SwaggerException(message, status, response, headers, null);
}
