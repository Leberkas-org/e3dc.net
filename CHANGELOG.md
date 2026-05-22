# Changelog

## [0.1.0](https://github.com/Leberkas-org/e3dc.net/compare/v0.1.0...v0.1.0) (2026-05-22)


### Bug Fixes

* changed image urls for nuget.org ([aefe503](https://github.com/Leberkas-org/e3dc.net/commit/aefe50331de7166b4cfb396e6dd035728f2956d9))

## 0.1.0 (2026-05-22)


### Features

* 4-tab dashboard with Explorer, History, Request Builder, PVI/PM sections ([bfbbe8c](https://github.com/Leberkas-org/e3dc.net/commit/bfbbe8c5f08ebeff41445a2db8a9f1e69028949a))
* add builder API with compile-time safety, E3DC theme, mermaid diagrams ([7f7c377](https://github.com/Leberkas-org/e3dc.net/commit/7f7c3774e3e3c8629d9d2f53799a5ff89448abb0))
* add DCDC, EP, WB, SYS sections to dashboard + Explorer live tree ([d88efef](https://github.com/Leberkas-org/e3dc.net/commit/d88efef5fc301013c4aa523b889358b72d6dbd6b))
* add DCDC, HA, Sys, Um descriptors and DCDC/EP/WB snapshot parsers ([3205fdf](https://github.com/Leberkas-org/e3dc.net/commit/3205fdf5e0eb652158aa0f807bb2106225e44c21))
* add DcdcDeviceIndex and WbDeviceIndex to appsettings config ([37ebf02](https://github.com/Leberkas-org/e3dc.net/commit/37ebf021cd45cad82defbb6e5aaa1877cc59be07))
* add Descriptor.All arrays, polish docs, fix VitePress base path ([7bb325a](https://github.com/Leberkas-org/e3dc.net/commit/7bb325ad7dfa301c0607c0bf52472cfaff3ab622))
* add history chart UI, response routing, IRawItemsCommand, DB tag fixes ([0f3ceae](https://github.com/Leberkas-org/e3dc.net/commit/0f3ceae9f308a9d8b65bee46ca73e1f26e95bfab))
* add interactive auth flow diagram on authentication page ([5be4b7b](https://github.com/Leberkas-org/e3dc.net/commit/5be4b7b5ca9bf3339a90cdc0f26346cfe40da126))
* add OpenAPI contract, NSwag config, generated controllers + TS client ([2325ad6](https://github.com/Leberkas-org/e3dc.net/commit/2325ad67bc9d4ce6f68ab9531562cb8b66a57c61))
* add polling interval and history config to appsettings ([ca890ad](https://github.com/Leberkas-org/e3dc.net/commit/ca890ad01ce27aee4961f37fabd94614082996aa))
* add sample applications and make RscpConnection/RscpFlow public ([2e88ce9](https://github.com/Leberkas-org/e3dc.net/commit/2e88ce9d04c67c57a120e9eadc82be7a0e40737a))
* add strongly-typed E3dcOptions configuration class ([51cd482](https://github.com/Leberkas-org/e3dc.net/commit/51cd482314d7bfe3e8eae1b414ae36ad366c9fc6))
* add ToInverterSnapshot and ToPowerMeterSnapshot parsing ([a5c73ec](https://github.com/Leberkas-org/e3dc.net/commit/a5c73eca2bf739cf8b414d3b1e8e48e6aa1d4f8d))
* configurable device indices for BAT, PVI, PM in appsettings ([ed4befa](https://github.com/Leberkas-org/e3dc.net/commit/ed4befad1003a46b9fb721ed6550b843b3ed250b))
* define actor message protocol for Snapshot, Polling, and RscpGateway actors ([2a66b74](https://github.com/Leberkas-org/e3dc.net/commit/2a66b740755f6d6b316385d731f268480bde1c24))
* Flatpickr date picker, fix 1-based index labels for history chart ([0b017ce](https://github.com/Leberkas-org/e3dc.net/commit/0b017ce4a4d4a9db93a078cd7cce1ba3005c8edc))
* formatted X-axis labels for history chart (HH:MM, weekday, day-of-month) ([7313244](https://github.com/Leberkas-org/e3dc.net/commit/7313244960dc10a2616a9323f080d14bba63c84a))
* host Swagger UI at /swagger for the OpenAPI contract ([45bfa03](https://github.com/Leberkas-org/e3dc.net/commit/45bfa03d33047d98a09f660c87310215e65ea5ab))
* implement controller stubs inheriting NSwag-generated bases, delegating to actors ([c270729](https://github.com/Leberkas-org/e3dc.net/commit/c270729089ce78fa597524cb8573ed6d11150b7d))
* implement PollingActor — tiered polling with Akka timers ([668c68b](https://github.com/Leberkas-org/e3dc.net/commit/668c68baeecf23f399ecb1c5db6c714140be56e5))
* implement protocol layer, tags, messages, connection, flow, client, and typed snapshots ([5060b7b](https://github.com/Leberkas-org/e3dc.net/commit/5060b7b65486e54473bdf4027944450b00177ece))
* implement RscpGatewayActor — RSCP connection, response routing, ad-hoc requests ([16e75cc](https://github.com/Leberkas-org/e3dc.net/commit/16e75ccfb00ed6cbd6fdba0060123c646ed9783a))
* implement SnapshotActor — owns all dashboard state ([d42170e](https://github.com/Leberkas-org/e3dc.net/commit/d42170e3812b7d98501ea9836abbe637f42b61cb))
* move power history chart directly below energy flow schematic ([ca452a5](https://github.com/Leberkas-org/e3dc.net/commit/ca452a58fcd6aa06a0ec1b5a48240d8cacde5c15))
* NSwag generates TS, tsc compiles to plain ES module JS client ([2556a39](https://github.com/Leberkas-org/e3dc.net/commit/2556a399e36d8390b36963717342b9ff7cedb94c))
* rebuild Explorer with live tag tree and structured send responses ([ed723c3](https://github.com/Leberkas-org/e3dc.net/commit/ed723c3c50698a47eb442e656ad39819d0bb0776))
* rebuild Explorer with raw RSCP tree, on-demand fetch, and copy buttons ([41d2a3a](https://github.com/Leberkas-org/e3dc.net/commit/41d2a3ae4a7d74f74bb078b2adf1a80daa85166b))
* rebuild Request Builder as 3-panel RSCP protocol workbench with tag docs ([ea2f954](https://github.com/Leberkas-org/e3dc.net/commit/ea2f954d44a23541f50dfd01589f06f712a20a9a))
* rebuild Request Builder as three-panel RSCP protocol workbench ([ad5f0cd](https://github.com/Leberkas-org/e3dc.net/commit/ad5f0cdb71b50ce152fe104934c0d30c8622ec68))
* redesign dashboard UI, fix RSCP request tags for BAT/PVI containers ([0d900ba](https://github.com/Leberkas-org/e3dc.net/commit/0d900ba5cd5c92f8739c7689d4b32af2cd8e5c92))
* replace 3 samples with live Dashboard (ASP.NET + SSE + Akka.Streams) ([c546cd2](https://github.com/Leberkas-org/e3dc.net/commit/c546cd2316d52749f3dc2b155dea96a4513afe29))
* replace date input with nav arrows, clickable date, Today/Yesterday buttons ([cb1ec71](https://github.com/Leberkas-org/e3dc.net/commit/cb1ec71fbc3043c9a3d15e0583813ad1ea0aaacd))
* replace mermaid with LikeC4 interactive web components ([933a08c](https://github.com/Leberkas-org/e3dc.net/commit/933a08cd01f96468857edb1755962a9dff1a31be))
* scrollable dashboard, vertical flow pipes for portrait screens ([aabe15f](https://github.com/Leberkas-org/e3dc.net/commit/aabe15f06b045d88bec5c6fa6970fbf10643569b))
* slim Program.cs to 84 lines — DI, actors, SSE, static files only ([fecd41e](https://github.com/Leberkas-org/e3dc.net/commit/fecd41ee02fe858c29363607a6fbf1dbad17292d))
* theme Flatpickr to match dark UI, fix duplicate last X-axis label ([0f5d0e4](https://github.com/Leberkas-org/e3dc.net/commit/0f5d0e44378583b225dfadc715c5498bece6ed4b))
* tiered polling, demand-driven streams, new API endpoints ([048d862](https://github.com/Leberkas-org/e3dc.net/commit/048d8629f5b6ffa8e80de6d4430c6337ea0f8570))
* upgrade dashboard with energy flow diagram, power history chart, battery details ([ef0049b](https://github.com/Leberkas-org/e3dc.net/commit/ef0049b3692f8a3c3b1f6f621b9dbe1b486ff795))
* wire DCDC, EP, WB into actors, update OpenAPI contract + regenerate ([454da40](https://github.com/Leberkas-org/e3dc.net/commit/454da40f1e4ecff32ac81ebd6b0ca293cf828078))
* wire DCDC, EP, WB namespaces into dashboard actors ([e9702b0](https://github.com/Leberkas-org/e3dc.net/commit/e9702b0f6386377d260914ff4132777b592f691b))


### Bug Fixes

* add ESM type to package.json and fix dead links in architecture page ([621a797](https://github.com/Leberkas-org/e3dc.net/commit/621a797729104b2b1c2d72afec0456b513d0897b))
* auto-load today's history data when switching to History tab ([078a74a](https://github.com/Leberkas-org/e3dc.net/commit/078a74a7c3913613ed691f1647585c4b033eb214))
* avoid eager ReadDouble on short items, add ReadValue helper ([071246b](https://github.com/Leberkas-org/e3dc.net/commit/071246bd2bc469053726e1bde1ef2841161e1ccd))
* clear history chart canvas when no data is available ([e7510c9](https://github.com/Leberkas-org/e3dc.net/commit/e7510c971a772557ecc25e415e582c0a45384ffd))
* correct DB history request format (UInt64 timestamps, 0x060001xx sub-tags) ([db7c945](https://github.com/Leberkas-org/e3dc.net/commit/db7c94570cf30f36bc546f5d3c0d6be89c343215))
* handle container-wrapped PVI/PM values and merge snapshots across polling tiers ([dc3de36](https://github.com/Leberkas-org/e3dc.net/commit/dc3de36ac2919dc9f65064213c12347f93c07206))
* normalize period to lowercase in histLabel (API now returns PascalCase enum) ([9996bc6](https://github.com/Leberkas-org/e3dc.net/commit/9996bc6ca5761f868db3ce73a0f8945152037bce))
* pin likec4@1.33.0, load web component via static script tag ([8ec5fd3](https://github.com/Leberkas-org/e3dc.net/commit/8ec5fd38f59512bf78369bc610a59923432a3ab5))
* read DB_GRAPH_INDEX as Float32 (E3DC returns floats, not ints) ([4fd6e5b](https://github.com/Leberkas-org/e3dc.net/commit/4fd6e5b77afc419137c9a7d0f1ff877fa2f6913d))
* response routing converts RscpDataResponse to typed DTOs, add Newtonsoft.Json MVC support ([47ba589](https://github.com/Leberkas-org/e3dc.net/commit/47ba5895ea891e3eb398c00752ed6859eec6d411))
* restore README paths, VitePress sidebar and GitHub link after history rewrite ([75fdb7c](https://github.com/Leberkas-org/e3dc.net/commit/75fdb7cb639582099ffc465c61c5f8231f23711b))
* separate DB summary container from value data points in history chart ([33c0ed8](https://github.com/Leberkas-org/e3dc.net/commit/33c0ed841ef54fde3a6164e1f748fa329f5009c2))
* serve static openapi.yaml in Swagger UI instead of code-generated JSON ([81b38f5](https://github.com/Leberkas-org/e3dc.net/commit/81b38f5347bc0b425d8b6fa46a92b99702c957c2))
* snap history query start date to period boundary ([ab7589b](https://github.com/Leberkas-org/e3dc.net/commit/ab7589b57a48133fd41a771abc86b8598f312049))
* timezone-safe date formatting, adaptive date labels per period, debounce ([779be2d](https://github.com/Leberkas-org/e3dc.net/commit/779be2d8b986f08c84de4e5ed153edb0d5b96531))
* use ordinal position for history X-axis labels, not E3DC graph index ([9b82471](https://github.com/Leberkas-org/e3dc.net/commit/9b82471d591b02009b460a6c944ba49c02583913))
* vitepress base path ([af2f924](https://github.com/Leberkas-org/e3dc.net/commit/af2f924a49ebaa3fbd16936d93b55663ef489b60))
