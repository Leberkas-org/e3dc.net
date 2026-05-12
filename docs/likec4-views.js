import { jsx } from "react/jsx-runtime";
import { useCallback, useMemo, useRef, useSyncExternalStore } from "react";
import { LikeC4Model } from "@likec4/core/model";
import { LikeC4ModelProvider as LikeC4ModelProvider$1, LikeC4View as LikeC4View$1, ReactLikeC4 as ReactLikeC4$1 } from "likec4/react";
//#region likec4:plugin/default/icons.jsx
var Icons = {};
function IconRenderer({ node, ...props }) {
	const IconComponent = Icons[node.icon ?? ""];
	if (!IconComponent) return null;
	return jsx(IconComponent, props);
}
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/chunks/rolldown-runtime.mjs
var e$4 = Object.defineProperty, __name = (t, n) => e$4(t, `name`, {
	value: n,
	configurable: !0
});
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/chunks/libs/nanostores.mjs
var e$3 = [], t$4 = 0, n$3 = 0;
var atom = (r) => {
	let i = [], a = {
		get() {
			return a.lc || a.listen(() => {})(), a.value;
		},
		init: r,
		lc: 0,
		listen(n) {
			return a.lc = i.push(n), () => {
				for (let r = t$4 + 4; r < e$3.length;) e$3[r] === n ? e$3.splice(r, 4) : r += 4;
				let r = i.indexOf(n);
				~r && (i.splice(r, 1), --a.lc || a.off());
			};
		},
		notify(r, o) {
			n$3++;
			let s = !e$3.length;
			for (let t of i) e$3.push(t, a.value, r, o);
			if (s) {
				for (t$4 = 0; t$4 < e$3.length; t$4 += 4) e$3[t$4](e$3[t$4 + 1], e$3[t$4 + 2], e$3[t$4 + 3]);
				e$3.length = 0;
			}
		},
		off() {},
		set(e) {
			let t = a.value;
			t !== e && (a.value = e, a.notify(t));
		},
		subscribe(e) {
			let t = a.listen(e);
			return e(a.value), t;
		},
		value: r
	};
	return a;
};
var on = (e, t, n, r) => (e.events = e.events || {}, e.events[n + 10] || (e.events[n + 10] = r((t) => {
	e.events[n].reduceRight((e, t) => (t(e), e), {
		shared: {},
		...t
	});
})), e.events[n] = e.events[n] || [], e.events[n].push(t), () => {
	let r = e.events[n], i = r.indexOf(t);
	r.splice(i, 1), r.length || (delete e.events[n], e.events[n + 10](), delete e.events[n + 10]);
}), onMount = (e, t) => {
	let listener = (n) => {
		let r = t(n);
		r && e.events[6].push(r);
	};
	return on(e, listener, 5, (t) => {
		let n = e.listen;
		e.listen = (...r) => (!e.lc && !e.active && (e.active = !0, t()), n(...r));
		let r = e.off;
		return e.events[6] = [], e.off = () => {
			r(), setTimeout(() => {
				if (e.active && !e.lc) {
					e.active = !1;
					for (let t of e.events[6]) t();
					e.events[6] = [];
				}
			}, 1e3);
		}, () => {
			e.listen = n, e.off = r;
		};
	});
}, computedStore = (e, t, r) => {
	Array.isArray(e) || (e = [e]);
	let i, a, set = () => {
		if (a === n$3) return;
		a = n$3;
		let r = e.map((e) => e.get());
		if (!i || r.some((e, t) => e !== i[t])) {
			i = r;
			let e = t(...r);
			e && e.then && e.t ? e.then((e) => {
				i === r && o.set(e);
			}) : (o.set(e), a = n$3);
		}
	}, o = atom(void 0), s = o.get;
	o.get = () => (set(), s());
	let c, l = r ? () => {
		clearTimeout(c), c = setTimeout(set);
	} : set;
	return onMount(o, () => {
		let t = e.map((e) => e.listen(l));
		return set(), () => {
			for (let e of t) e();
		};
	}), o;
}, computed = (e, t) => computedStore(e, t);
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/chunks/libs/@nanostores/react.mjs
function listenKeys(e, t, n) {
	let r = new Set(t).add(void 0);
	return e.listen((e, t, i) => {
		r.has(i) && n(e, t, i);
	});
}
var emit = (e, t) => (n) => {
	e.current !== n && (e.current = n, t());
};
function useStore(r, { keys: i, deps: a = [r, i], ssr: o } = {}) {
	let s = useRef();
	s.current = r.get();
	let c = useCallback((e) => (emit(s, e)(r.value), i?.length > 0 ? listenKeys(r, i, emit(s, e)) : r.listen(emit(s, e))), a), get = () => s.current, l = get;
	return o && `init` in r && (l = o === `initial` ? () => r.init : o), useSyncExternalStore(c, get, l);
}
Math.random.bind(Math);
var { clearTimeout: n$2, setTimeout: r$1 } = globalThis;
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/chunks/libs/fast-equals.mjs
var { getOwnPropertyNames: e$1, getOwnPropertySymbols: t$2 } = Object, { hasOwnProperty: n$1 } = Object.prototype;
function combineComparators(e, t) {
	return function isEqual(n, r, i) {
		return e(n, r, i) && t(n, r, i);
	};
}
function createIsCircular(e) {
	return function isCircular(t, n, r) {
		if (!t || !n || typeof t != `object` || typeof n != `object`) return e(t, n, r);
		let { cache: i } = r, a = i.get(t), o = i.get(n);
		if (a && o) return a === n && o === t;
		i.set(t, n), i.set(n, t);
		let s = e(t, n, r);
		return i.delete(t), i.delete(n), s;
	};
}
function getStrictProperties(n) {
	return e$1(n).concat(t$2(n));
}
var r = Object.hasOwn || ((e, t) => n$1.call(e, t)), { getOwnPropertyDescriptor: i, keys: a } = Object, o = Object.is || function sameValueEqual(e, t) {
	return e === t ? e !== 0 || 1 / e == 1 / t : e !== e && t !== t;
};
function strictEqual(e, t) {
	return e === t;
}
function areArrayBuffersEqual(e, t) {
	return e.byteLength === t.byteLength && areTypedArraysEqual(new Uint8Array(e), new Uint8Array(t));
}
function areArraysEqual(e, t, n) {
	let r = e.length;
	if (t.length !== r) return !1;
	for (; r-- > 0;) if (!n.equals(e[r], t[r], r, r, e, t, n)) return !1;
	return !0;
}
function areDataViewsEqual(e, t) {
	return e.byteLength === t.byteLength && areTypedArraysEqual(new Uint8Array(e.buffer, e.byteOffset, e.byteLength), new Uint8Array(t.buffer, t.byteOffset, t.byteLength));
}
function areDatesEqual(e, t) {
	return o(e.getTime(), t.getTime());
}
function areErrorsEqual(e, t) {
	return e.name === t.name && e.message === t.message && e.cause === t.cause && e.stack === t.stack;
}
function areMapsEqual(e, t, n) {
	let r = e.size;
	if (r !== t.size) return !1;
	if (!r) return !0;
	let i = Array(r), a = e.entries(), o, s, c = 0;
	for (; (o = a.next()) && !o.done;) {
		let r = t.entries(), a = !1, l = 0;
		for (; (s = r.next()) && !s.done;) {
			if (i[l]) {
				l++;
				continue;
			}
			let r = o.value, u = s.value;
			if (n.equals(r[0], u[0], c, l, e, t, n) && n.equals(r[1], u[1], r[0], u[0], e, t, n)) {
				a = i[l] = !0;
				break;
			}
			l++;
		}
		if (!a) return !1;
		c++;
	}
	return !0;
}
function areObjectsEqual(e, t, n) {
	let r = a(e), i = r.length;
	if (a(t).length !== i) return !1;
	for (; i-- > 0;) if (!isPropertyEqual(e, t, n, r[i])) return !1;
	return !0;
}
function areObjectsEqualStrict(e, t, n) {
	let r = getStrictProperties(e), a = r.length;
	if (getStrictProperties(t).length !== a) return !1;
	let o, s, c;
	for (; a-- > 0;) if (o = r[a], !isPropertyEqual(e, t, n, o) || (s = i(e, o), c = i(t, o), (s || c) && (!s || !c || s.configurable !== c.configurable || s.enumerable !== c.enumerable || s.writable !== c.writable))) return !1;
	return !0;
}
function arePrimitiveWrappersEqual(e, t) {
	return o(e.valueOf(), t.valueOf());
}
function areRegExpsEqual(e, t) {
	return e.source === t.source && e.flags === t.flags;
}
function areSetsEqual(e, t, n) {
	let r = e.size;
	if (r !== t.size) return !1;
	if (!r) return !0;
	let i = Array(r), a = e.values(), o, s;
	for (; (o = a.next()) && !o.done;) {
		let r = t.values(), a = !1, c = 0;
		for (; (s = r.next()) && !s.done;) {
			if (!i[c] && n.equals(o.value, s.value, o.value, s.value, e, t, n)) {
				a = i[c] = !0;
				break;
			}
			c++;
		}
		if (!a) return !1;
	}
	return !0;
}
function areTypedArraysEqual(e, t) {
	let n = e.byteLength;
	if (t.byteLength !== n || e.byteOffset !== t.byteOffset) return !1;
	for (; n-- > 0;) if (e[n] !== t[n]) return !1;
	return !0;
}
function areUrlsEqual(e, t) {
	return e.hostname === t.hostname && e.pathname === t.pathname && e.protocol === t.protocol && e.port === t.port && e.hash === t.hash && e.username === t.username && e.password === t.password;
}
function isPropertyEqual(e, t, n, i) {
	return (i === `_owner` || i === `__o` || i === `__v`) && (e.$$typeof || t.$$typeof) ? !0 : r(t, i) && n.equals(e[i], t[i], i, i, e, t, n);
}
var s = Object.prototype.toString;
function createEqualityComparator(e) {
	let t = createSupportedComparatorMap(e), { areArraysEqual: n, areDatesEqual: r, areFunctionsEqual: i, areMapsEqual: a, areNumbersEqual: o, areObjectsEqual: c, areRegExpsEqual: l, areSetsEqual: u, getUnsupportedCustomComparator: d } = e;
	return function comparator(e, f, p) {
		if (e === f) return !0;
		if (e == null || f == null) return !1;
		let m = typeof e;
		if (m !== typeof f) return !1;
		if (m !== `object`) return m === `number` || m === `bigint` ? o(e, f, p) : m === `function` ? i(e, f, p) : !1;
		let h = e.constructor;
		if (h !== f.constructor) return !1;
		if (h === Object) return c(e, f, p);
		if (h === Array) return n(e, f, p);
		if (h === Date) return r(e, f, p);
		if (h === RegExp) return l(e, f, p);
		if (h === Map) return a(e, f, p);
		if (h === Set) return u(e, f, p);
		if (h === Promise) return !1;
		if (Array.isArray(e)) return n(e, f, p);
		let g = s.call(e), _ = t[g];
		if (_) return _(e, f, p);
		let v = d && d(e, f, p, g);
		return v ? v(e, f, p) : !1;
	};
}
function createEqualityComparatorConfig({ circular: e, createCustomConfig: t, strict: n }) {
	let r = {
		areArrayBuffersEqual,
		areArraysEqual: n ? areObjectsEqualStrict : areArraysEqual,
		areDataViewsEqual,
		areDatesEqual,
		areErrorsEqual,
		areFunctionsEqual: strictEqual,
		areMapsEqual: n ? combineComparators(areMapsEqual, areObjectsEqualStrict) : areMapsEqual,
		areNumbersEqual: o,
		areObjectsEqual: n ? areObjectsEqualStrict : areObjectsEqual,
		arePrimitiveWrappersEqual,
		areRegExpsEqual,
		areSetsEqual: n ? combineComparators(areSetsEqual, areObjectsEqualStrict) : areSetsEqual,
		areTypedArraysEqual: n ? combineComparators(areTypedArraysEqual, areObjectsEqualStrict) : areTypedArraysEqual,
		areUrlsEqual,
		getUnsupportedCustomComparator: void 0
	};
	if (t && (r = Object.assign({}, r, t(r))), e) {
		let e = createIsCircular(r.areArraysEqual), t = createIsCircular(r.areMapsEqual), n = createIsCircular(r.areObjectsEqual), i = createIsCircular(r.areSetsEqual);
		r = Object.assign({}, r, {
			areArraysEqual: e,
			areMapsEqual: t,
			areObjectsEqual: n,
			areSetsEqual: i
		});
	}
	return r;
}
function createInternalEqualityComparator(e) {
	return function(t, n, r, i, a, o, s) {
		return e(t, n, s);
	};
}
function createIsEqual({ circular: e, comparator: t, createState: n, equals: r, strict: i }) {
	if (n) return function isEqual(a, o) {
		let { cache: s = e ? /* @__PURE__ */ new WeakMap() : void 0, meta: c } = n();
		return t(a, o, {
			cache: s,
			equals: r,
			meta: c,
			strict: i
		});
	};
	if (e) return function isEqual(e, n) {
		return t(e, n, {
			cache: /* @__PURE__ */ new WeakMap(),
			equals: r,
			meta: void 0,
			strict: i
		});
	};
	let a = {
		cache: void 0,
		equals: r,
		meta: void 0,
		strict: i
	};
	return function isEqual(e, n) {
		return t(e, n, a);
	};
}
function createSupportedComparatorMap({ areArrayBuffersEqual: e, areArraysEqual: t, areDataViewsEqual: n, areDatesEqual: r, areErrorsEqual: i, areFunctionsEqual: a, areMapsEqual: o, areNumbersEqual: s, areObjectsEqual: c, arePrimitiveWrappersEqual: l, areRegExpsEqual: u, areSetsEqual: d, areTypedArraysEqual: f, areUrlsEqual: p }) {
	return {
		"[object Arguments]": c,
		"[object Array]": t,
		"[object ArrayBuffer]": e,
		"[object AsyncGeneratorFunction]": a,
		"[object BigInt]": s,
		"[object BigInt64Array]": f,
		"[object BigUint64Array]": f,
		"[object Boolean]": l,
		"[object DataView]": n,
		"[object Date]": r,
		"[object Error]": i,
		"[object Float16Array]": f,
		"[object Float32Array]": f,
		"[object Float64Array]": f,
		"[object Function]": a,
		"[object GeneratorFunction]": a,
		"[object Int8Array]": f,
		"[object Int16Array]": f,
		"[object Int32Array]": f,
		"[object Map]": o,
		"[object Number]": l,
		"[object Object]": (e, t, n) => typeof e.then != `function` && typeof t.then != `function` && c(e, t, n),
		"[object RegExp]": u,
		"[object Set]": d,
		"[object String]": l,
		"[object URL]": p,
		"[object Uint8Array]": f,
		"[object Uint8ClampedArray]": f,
		"[object Uint16Array]": f,
		"[object Uint32Array]": f
	};
}
var c = createCustomEqual();
createCustomEqual({ strict: !0 }), createCustomEqual({ circular: !0 }), createCustomEqual({
	circular: !0,
	strict: !0
});
var l = createCustomEqual({ createInternalComparator: () => o });
createCustomEqual({
	strict: !0,
	createInternalComparator: () => o
}), createCustomEqual({
	circular: !0,
	createInternalComparator: () => o
}), createCustomEqual({
	circular: !0,
	createInternalComparator: () => o,
	strict: !0
});
function createCustomEqual(e = {}) {
	let { circular: t = !1, createInternalComparator: n, createState: r, strict: i = !1 } = e, a = createEqualityComparator(createEqualityComparatorConfig(e));
	return createIsEqual({
		circular: t,
		comparator: a,
		createState: r,
		equals: n ? n(a) : createInternalEqualityComparator(a),
		strict: i
	});
}
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/chunks/libs/remeda.mjs
function e(i, a, o) {
	let r = (o) => i(o, ...a);
	return o === void 0 ? r : Object.assign(r, {
		lazy: o,
		lazyArgs: a
	});
}
function t$1(i, a, o) {
	let s = i.length - a.length;
	if (s === 0) return i(...a);
	if (s === 1) return e(i, a, o);
	throw Error(`Wrong number of arguments`);
}
__name(t$1, `t`);
function t(...i) {
	return t$1(n, i);
}
function n(i, a) {
	let o = {};
	for (let [s, c] of Object.entries(i)) o[s] = a(c, s, i);
	return o;
}
//#endregion
//#region node_modules/likec4/dist/vite-plugin/internal/index.mjs
function createHooksForModel(e) {
	let t$6 = computed(e, (e) => LikeC4Model.create(e));
	function updateModel(t$5) {
		let n = e.get(), r = {
			...t$5,
			views: t(t$5.views, (e) => {
				let t = n.views[e.id];
				return c(t, e) ? t : e;
			})
		};
		l(r.views, n.views) && c(r, n) || e.set(r);
	}
	let r = computed(t$6, (e) => [...e.views()].map((e) => e.$layouted));
	function useLikeC4Model() {
		return useStore(t$6);
	}
	function useLikeC4Views() {
		return useStore(r);
	}
	function useLikeC4View(e) {
		return useStore(useMemo(() => computed(t$6, (t) => t.findView(e)?.$layouted ?? null), [e]));
	}
	return {
		updateModel,
		$likec4model: t$6,
		useLikeC4Model,
		useLikeC4Views,
		useLikeC4View
	};
}
var { updateModel, $likec4model, useLikeC4Model, useLikeC4Views, useLikeC4View } = createHooksForModel(atom({
	_stage: "layouted",
	projectId: "default",
	project: {
		id: "default",
		title: "default"
	},
	specification: {
		tags: {},
		elements: {
			actor: { style: {} },
			system: { style: {} },
			component: { style: {} }
		},
		relationships: {},
		deployments: {},
		customColors: {}
	},
	elements: {
		user: {
			style: {},
			description: { txt: "Consumer of the e3dc-connector library" },
			title: "Your Application",
			kind: "actor",
			id: "user"
		},
		e3dc: {
			style: {},
			description: { txt: "Home battery system with RSCP protocol on TCP:5033" },
			title: "E3DC S10 Pro",
			kind: "system",
			id: "e3dc"
		},
		connector: {
			style: {},
			description: { txt: ".NET Akka.Streams RSCP client library" },
			title: "e3dc-connector",
			kind: "system",
			id: "connector"
		},
		"connector.client": {
			style: {},
			description: { txt: "Correlation-based request-reply via channels" },
			title: "RscpClient",
			kind: "component",
			id: "connector.client"
		},
		"connector.flow": {
			style: {},
			description: { txt: "Akka.Streams pipeline wrapped in RestartFlow" },
			title: "RscpFlow",
			kind: "component",
			id: "connector.flow"
		},
		"connector.connection": {
			style: {},
			description: { txt: "TCP socket, Rijndael-256 CBC state, auth handshake" },
			title: "RscpConnection",
			kind: "component",
			id: "connector.connection"
		},
		"connector.protocol": {
			style: {},
			description: { txt: "RscpFrame, RscpDataItem, RscpCrypt, RscpDataType" },
			title: "Protocol Layer",
			kind: "component",
			id: "connector.protocol"
		},
		"connector.typed": {
			style: {},
			description: { txt: "EmsPowerSnapshot, BatterySnapshot, InverterSnapshot" },
			title: "Typed Layer",
			kind: "component",
			id: "connector.typed"
		},
		"connector.flow.merge": {
			style: {},
			description: { txt: "Merges polling tick + on-demand commands" },
			title: "MergePreferred",
			kind: "component",
			id: "connector.flow.merge"
		},
		"connector.flow.encode": {
			style: {},
			description: { txt: "IRscpCommand to RscpFrame bytes" },
			title: "EncodeStage",
			kind: "component",
			id: "connector.flow.encode"
		},
		"connector.flow.execute": {
			style: {},
			description: { txt: "TCP send/receive with Rijndael-256" },
			title: "ExecuteStage",
			kind: "component",
			id: "connector.flow.execute"
		},
		"connector.flow.decode": {
			style: {},
			description: { txt: "RscpFrame bytes to IRscpMessage" },
			title: "DecodeStage",
			kind: "component",
			id: "connector.flow.decode"
		}
	},
	relations: {
		po5f95: {
			title: "SendAsync / Subscribe",
			source: { model: "user" },
			target: { model: "connector.client" },
			id: "po5f95"
		},
		b9v84w: {
			title: "ChannelWriter",
			source: { model: "connector.client" },
			target: { model: "connector.flow" },
			id: "b9v84w"
		},
		"1miu1d": {
			title: "ChannelReader",
			source: { model: "connector.flow" },
			target: { model: "connector.client" },
			id: "1miu1d"
		},
		dk4spz: {
			title: "commands",
			source: { model: "connector.flow.merge" },
			target: { model: "connector.flow.encode" },
			id: "dk4spz"
		},
		vocce9: {
			title: "frame bytes",
			source: { model: "connector.flow.encode" },
			target: { model: "connector.flow.execute" },
			id: "vocce9"
		},
		"1iziy3o": {
			title: "response bytes",
			source: { model: "connector.flow.execute" },
			target: { model: "connector.flow.decode" },
			id: "1iziy3o"
		},
		c0ivc3: {
			title: "SendFrame / ReceiveFrame",
			source: { model: "connector.flow.execute" },
			target: { model: "connector.connection" },
			id: "c0ivc3"
		},
		pnptou: {
			title: "Serialize / Deserialize",
			source: { model: "connector.connection" },
			target: { model: "connector.protocol" },
			id: "pnptou"
		},
		"1pww079": {
			title: "TCP:5033 Rijndael-256 CBC",
			source: { model: "connector.connection" },
			target: { model: "e3dc" },
			id: "1pww079"
		},
		"17xigf4": {
			title: "Parse tag responses",
			source: { model: "connector.flow.decode" },
			target: { model: "connector.typed" },
			id: "17xigf4"
		}
	},
	globals: {
		predicates: {},
		dynamicPredicates: {},
		styles: {}
	},
	views: {
		index: {
			_stage: "layouted",
			_type: "element",
			id: "index",
			title: "Landscape view",
			description: null,
			autoLayout: { direction: "TB" },
			hash: "8VSLSJc9AA15Xj5Bmsb-Zy7GGp3GFKBtjLAcjylGchc",
			bounds: {
				x: 0,
				y: 0,
				width: 353,
				height: 826
			},
			nodes: [
				{
					id: "user",
					parent: null,
					level: 0,
					children: [],
					inEdges: [],
					outEdges: ["16sb508"],
					title: "Your Application",
					modelRef: "user",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Consumer of the e3dc-connector library" },
					tags: [],
					kind: "actor",
					x: 8,
					y: 0,
					width: 320,
					height: 180,
					labelBBox: {
						x: 27,
						y: 65,
						width: 267,
						height: 47
					}
				},
				{
					id: "connector",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["16sb508"],
					outEdges: ["r3ohlk"],
					title: "e3dc-connector",
					modelRef: "connector",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: ".NET Akka.Streams RSCP client library" },
					tags: [],
					kind: "system",
					navigateTo: "systemContext",
					x: 8,
					y: 323,
					width: 320,
					height: 180,
					labelBBox: {
						x: 28,
						y: 65,
						width: 266,
						height: 47
					}
				},
				{
					id: "e3dc",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["r3ohlk"],
					outEdges: [],
					title: "E3DC S10 Pro",
					modelRef: "e3dc",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Home battery system with RSCP protocol on TCP:5033" },
					tags: [],
					kind: "system",
					x: 0,
					y: 646,
					width: 336,
					height: 180,
					labelBBox: {
						x: 18,
						y: 56,
						width: 301,
						height: 65
					}
				}
			],
			edges: [{
				id: "16sb508",
				parent: null,
				source: "user",
				target: "connector",
				label: "SendAsync / Subscribe",
				relations: ["po5f95"],
				color: "gray",
				line: "dashed",
				head: "normal",
				points: [
					[168, 180],
					[168, 221],
					[168, 270],
					[168, 313]
				],
				labelBBox: {
					x: 169,
					y: 241,
					width: 149,
					height: 18
				}
			}, {
				id: "r3ohlk",
				parent: null,
				source: "connector",
				target: "e3dc",
				label: "TCP:5033 Rijndael-256 CBC",
				relations: ["1pww079"],
				color: "gray",
				line: "dashed",
				head: "normal",
				points: [
					[168, 503],
					[168, 544],
					[168, 593],
					[168, 635]
				],
				labelBBox: {
					x: 169,
					y: 564,
					width: 183,
					height: 18
				}
			}]
		},
		systemContext: {
			_type: "element",
			tags: null,
			links: null,
			viewOf: "connector",
			_stage: "layouted",
			sourcePath: "architecture/likec4/views.c4",
			description: null,
			title: "System Context",
			id: "systemContext",
			autoLayout: { direction: "TB" },
			hash: "2iG4x6wvicSVaWnzTsylFVLnljhS84bV94iK02iisSU",
			bounds: {
				x: 0,
				y: 0,
				width: 353,
				height: 826
			},
			nodes: [
				{
					id: "user",
					parent: null,
					level: 0,
					children: [],
					inEdges: [],
					outEdges: ["16sb508"],
					title: "Your Application",
					modelRef: "user",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Consumer of the e3dc-connector library" },
					tags: [],
					kind: "actor",
					x: 8,
					y: 0,
					width: 320,
					height: 180,
					labelBBox: {
						x: 27,
						y: 65,
						width: 267,
						height: 47
					}
				},
				{
					id: "connector",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["16sb508"],
					outEdges: ["r3ohlk"],
					title: "e3dc-connector",
					modelRef: "connector",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: ".NET Akka.Streams RSCP client library" },
					tags: [],
					kind: "system",
					navigateTo: "layers",
					x: 8,
					y: 323,
					width: 320,
					height: 180,
					labelBBox: {
						x: 28,
						y: 65,
						width: 266,
						height: 47
					}
				},
				{
					id: "e3dc",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["r3ohlk"],
					outEdges: [],
					title: "E3DC S10 Pro",
					modelRef: "e3dc",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Home battery system with RSCP protocol on TCP:5033" },
					tags: [],
					kind: "system",
					x: 0,
					y: 646,
					width: 336,
					height: 180,
					labelBBox: {
						x: 18,
						y: 56,
						width: 301,
						height: 65
					}
				}
			],
			edges: [{
				id: "16sb508",
				parent: null,
				source: "user",
				target: "connector",
				label: "SendAsync / Subscribe",
				relations: ["po5f95"],
				color: "gray",
				line: "dashed",
				head: "normal",
				points: [
					[168, 180],
					[168, 221],
					[168, 270],
					[168, 313]
				],
				labelBBox: {
					x: 169,
					y: 241,
					width: 149,
					height: 18
				}
			}, {
				id: "r3ohlk",
				parent: null,
				source: "connector",
				target: "e3dc",
				label: "TCP:5033 Rijndael-256 CBC",
				relations: ["1pww079"],
				color: "gray",
				line: "dashed",
				head: "normal",
				points: [
					[168, 503],
					[168, 544],
					[168, 593],
					[168, 635]
				],
				labelBBox: {
					x: 169,
					y: 564,
					width: 183,
					height: 18
				}
			}]
		},
		pipeline: {
			_type: "element",
			tags: null,
			links: null,
			viewOf: "connector.flow",
			_stage: "layouted",
			sourcePath: "architecture/likec4/views.c4",
			description: null,
			title: "Akka.Streams Pipeline",
			id: "pipeline",
			autoLayout: { direction: "TB" },
			hash: "JOZhbI3Fb0xWj-inkahnFhnydSL0etUiktlMSJBoyNk",
			bounds: {
				x: 0,
				y: 0,
				width: 971,
				height: 1519
			},
			nodes: [
				{
					id: "connector.client",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["8h49fl"],
					outEdges: ["14ljlnl"],
					title: "RscpClient",
					modelRef: "connector.client",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Correlation-based request-reply via channels" },
					tags: [],
					kind: "component",
					x: 424,
					y: 1339,
					width: 338,
					height: 180,
					labelBBox: {
						x: 18,
						y: 65,
						width: 302,
						height: 47
					}
				},
				{
					id: "connector.flow",
					parent: null,
					level: 0,
					children: [
						"connector.flow.merge",
						"connector.flow.encode",
						"connector.flow.execute",
						"connector.flow.decode"
					],
					inEdges: ["14ljlnl"],
					outEdges: ["8h49fl", "yodchr"],
					title: "RscpFlow",
					modelRef: "connector.flow",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Akka.Streams pipeline wrapped in RestartFlow" },
					tags: [],
					kind: "component",
					depth: 1,
					x: 8,
					y: 8,
					width: 414,
					height: 1250,
					labelBBox: {
						x: 6,
						y: 0,
						width: 67,
						height: 15
					}
				},
				{
					id: "connector.flow.merge",
					parent: "connector.flow",
					level: 1,
					children: [],
					inEdges: [],
					outEdges: ["1kzdufo"],
					title: "MergePreferred",
					modelRef: "connector.flow.merge",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Merges polling tick + on-demand commands" },
					tags: [],
					kind: "component",
					x: 48,
					y: 69,
					width: 334,
					height: 180,
					labelBBox: {
						x: 18,
						y: 65,
						width: 299,
						height: 47
					}
				},
				{
					id: "connector.flow.encode",
					parent: "connector.flow",
					level: 1,
					children: [],
					inEdges: ["1kzdufo"],
					outEdges: ["1lqsqmr"],
					title: "EncodeStage",
					modelRef: "connector.flow.encode",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "IRscpCommand to RscpFrame bytes" },
					tags: [],
					kind: "component",
					x: 55,
					y: 392,
					width: 320,
					height: 180,
					labelBBox: {
						x: 35,
						y: 65,
						width: 250,
						height: 47
					}
				},
				{
					id: "connector.flow.execute",
					parent: "connector.flow",
					level: 1,
					children: [],
					inEdges: ["1lqsqmr"],
					outEdges: ["yzvc2h", "yodchr"],
					title: "ExecuteStage",
					modelRef: "connector.flow.execute",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "TCP send/receive with Rijndael-256" },
					tags: [],
					kind: "component",
					x: 55,
					y: 715,
					width: 320,
					height: 180,
					labelBBox: {
						x: 39,
						y: 65,
						width: 242,
						height: 47
					}
				},
				{
					id: "connector.flow.decode",
					parent: "connector.flow",
					level: 1,
					children: [],
					inEdges: ["yzvc2h"],
					outEdges: [],
					title: "DecodeStage",
					modelRef: "connector.flow.decode",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "RscpFrame bytes to IRscpMessage" },
					tags: [],
					kind: "component",
					x: 55,
					y: 1038,
					width: 320,
					height: 180,
					labelBBox: {
						x: 39,
						y: 65,
						width: 242,
						height: 47
					}
				},
				{
					id: "connector.connection",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["yodchr"],
					outEdges: [],
					title: "RscpConnection",
					modelRef: "connector.connection",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "TCP socket, Rijndael-256 CBC state, auth handshake" },
					tags: [],
					kind: "component",
					x: 485,
					y: 1038,
					width: 320,
					height: 180,
					labelBBox: {
						x: 18,
						y: 56,
						width: 284,
						height: 65
					}
				}
			],
			edges: [
				{
					id: "1kzdufo",
					parent: "connector.flow",
					source: "connector.flow.merge",
					target: "connector.flow.encode",
					label: "commands",
					relations: ["dk4spz"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[215, 249],
						[215, 290],
						[215, 339],
						[215, 382]
					],
					labelBBox: {
						x: 216,
						y: 310,
						width: 72,
						height: 18
					}
				},
				{
					id: "1lqsqmr",
					parent: "connector.flow",
					source: "connector.flow.encode",
					target: "connector.flow.execute",
					label: "frame bytes",
					relations: ["vocce9"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[215, 572],
						[215, 613],
						[215, 662],
						[215, 705]
					],
					labelBBox: {
						x: 216,
						y: 633,
						width: 77,
						height: 18
					}
				},
				{
					id: "yzvc2h",
					parent: "connector.flow",
					source: "connector.flow.execute",
					target: "connector.flow.decode",
					label: "response bytes",
					relations: ["1iziy3o"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[215, 895],
						[215, 936],
						[215, 985],
						[215, 1027]
					],
					labelBBox: {
						x: 216,
						y: 956,
						width: 99,
						height: 18
					}
				},
				{
					id: "yodchr",
					parent: null,
					source: "connector.flow.execute",
					target: "connector.connection",
					label: "SendFrame / ReceiveFrame",
					relations: ["c0ivc3"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[334, 895],
						[391, 937],
						[460, 988],
						[518, 1032]
					],
					labelBBox: {
						x: 444,
						y: 956,
						width: 180,
						height: 18
					}
				},
				{
					id: "14ljlnl",
					parent: null,
					source: "connector.client",
					target: "connector.flow",
					label: "ChannelWriter",
					relations: ["b9v84w"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[762, 1368],
						[863, 1321],
						[971, 1243],
						[971, 1129],
						[971, 481],
						[971, 481],
						[971, 481],
						[971, 363],
						[655, 264],
						[432, 208]
					],
					labelBBox: {
						x: 877,
						y: 540,
						width: 93,
						height: 18
					}
				},
				{
					id: "8h49fl",
					parent: null,
					source: "connector.flow",
					target: "connector.client",
					label: "ChannelReader",
					relations: ["1miu1d"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[378, 1258],
						[410, 1283],
						[443, 1309],
						[473, 1332]
					],
					labelBBox: {
						x: 326,
						y: 1276,
						width: 102,
						height: 18
					}
				}
			]
		},
		layers: {
			_type: "element",
			tags: null,
			links: null,
			viewOf: "connector",
			_stage: "layouted",
			sourcePath: "architecture/likec4/views.c4",
			description: null,
			title: "Protocol Layers",
			id: "layers",
			autoLayout: { direction: "TB" },
			hash: "AWcbHL-ia4tzfuTScNQUvHdhVgWYjGjBDyHea-1HZQw",
			bounds: {
				x: 0,
				y: 0,
				width: 1197,
				height: 826
			},
			nodes: [
				{
					id: "connector.flow",
					parent: null,
					level: 0,
					children: [],
					inEdges: [],
					outEdges: ["1yymeuc", "1wrfvfy"],
					title: "RscpFlow",
					modelRef: "connector.flow",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Akka.Streams pipeline wrapped in RestartFlow" },
					tags: [],
					kind: "component",
					navigateTo: "pipeline",
					x: 414,
					y: 0,
					width: 351,
					height: 180,
					labelBBox: {
						x: 19,
						y: 65,
						width: 315,
						height: 47
					}
				},
				{
					id: "connector.typed",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["1yymeuc"],
					outEdges: [],
					title: "Typed Layer",
					modelRef: "connector.typed",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "EmsPowerSnapshot, BatterySnapshot, InverterSnapshot" },
					tags: [],
					kind: "component",
					x: 0,
					y: 323,
					width: 320,
					height: 180,
					labelBBox: {
						x: 28,
						y: 56,
						width: 263,
						height: 65
					}
				},
				{
					id: "connector.connection",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["1wrfvfy"],
					outEdges: ["1pvcfai", "12u46cg"],
					title: "RscpConnection",
					modelRef: "connector.connection",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "TCP socket, Rijndael-256 CBC state, auth handshake" },
					tags: [],
					kind: "component",
					x: 430,
					y: 323,
					width: 320,
					height: 180,
					labelBBox: {
						x: 18,
						y: 56,
						width: 284,
						height: 65
					}
				},
				{
					id: "connector.protocol",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["1pvcfai"],
					outEdges: [],
					title: "Protocol Layer",
					modelRef: "connector.protocol",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "RscpFrame, RscpDataItem, RscpCrypt, RscpDataType" },
					tags: [],
					kind: "component",
					x: 430,
					y: 646,
					width: 320,
					height: 180,
					labelBBox: {
						x: 26,
						y: 56,
						width: 267,
						height: 65
					}
				},
				{
					id: "e3dc",
					parent: null,
					level: 0,
					children: [],
					inEdges: ["12u46cg"],
					outEdges: [],
					title: "E3DC S10 Pro",
					modelRef: "e3dc",
					shape: "rectangle",
					color: "primary",
					style: {
						opacity: 15,
						size: "md"
					},
					description: { txt: "Home battery system with RSCP protocol on TCP:5033" },
					tags: [],
					kind: "system",
					x: 860,
					y: 646,
					width: 336,
					height: 180,
					labelBBox: {
						x: 18,
						y: 56,
						width: 301,
						height: 65
					}
				}
			],
			edges: [
				{
					id: "1yymeuc",
					parent: null,
					source: "connector.flow",
					target: "connector.typed",
					label: "Parse tag responses",
					relations: ["17xigf4"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[471, 180],
						[414, 223],
						[345, 274],
						[287, 317]
					],
					labelBBox: {
						x: 389,
						y: 241,
						width: 132,
						height: 18
					}
				},
				{
					id: "1wrfvfy",
					parent: null,
					source: "connector.flow",
					target: "connector.connection",
					label: "SendFrame / ReceiveFrame",
					relations: ["c0ivc3"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[590, 180],
						[590, 221],
						[590, 270],
						[590, 313]
					],
					labelBBox: {
						x: 591,
						y: 241,
						width: 180,
						height: 18
					}
				},
				{
					id: "1pvcfai",
					parent: null,
					source: "connector.connection",
					target: "connector.protocol",
					label: "Serialize / Deserialize",
					relations: ["pnptou"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[590, 503],
						[590, 544],
						[590, 593],
						[590, 635]
					],
					labelBBox: {
						x: 591,
						y: 564,
						width: 139,
						height: 18
					}
				},
				{
					id: "12u46cg",
					parent: null,
					source: "connector.connection",
					target: "e3dc",
					label: "TCP:5033 Rijndael-256 CBC",
					relations: ["1pww079"],
					color: "gray",
					line: "dashed",
					head: "normal",
					points: [
						[711, 503],
						[770, 545],
						[839, 596],
						[898, 640]
					],
					labelBBox: {
						x: 823,
						y: 564,
						width: 183,
						height: 18
					}
				}
			]
		}
	},
	deployments: {
		elements: {},
		relations: {}
	},
	imports: {},
	manualLayouts: {}
}));
//#endregion
//#region likec4:plugin/default/react.js
function LikeC4ModelProvider({ children }) {
	return jsx(LikeC4ModelProvider$1, {
		likec4model: useLikeC4Model(),
		children
	});
}
function LikeC4View(props) {
	return jsx(LikeC4ModelProvider, { children: jsx(LikeC4View$1, {
		renderIcon: IconRenderer,
		...props
	}) });
}
function ReactLikeC4(props) {
	return jsx(LikeC4ModelProvider, { children: jsx(ReactLikeC4$1, {
		renderIcon: IconRenderer,
		...props
	}) });
}
//#endregion
//#region node_modules/likec4/__app__/codegen/react.mjs
var likec4model = $likec4model.get();
function isLikeC4ViewId(value) {
	return value != null && typeof value === "string" && !!likec4model.findView(value);
}
//#endregion
export { LikeC4ModelProvider, LikeC4View, ReactLikeC4, IconRenderer as RenderIcon, isLikeC4ViewId, likec4model, useLikeC4Model, useLikeC4View };
