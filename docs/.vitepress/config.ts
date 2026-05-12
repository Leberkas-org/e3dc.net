import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'E3DC Connector',
  description: 'Akka.Streams RSCP client for E3DC S10 Pro',
  vue: {
    template: {
      compilerOptions: {
        isCustomElement: (tag) => tag === 'likec4-view',
      },
    },
  },
  head: [
    ['script', { src: '/likec4-views.js', defer: 'true' }],
    ['style', {}, `
      :root {
        --vp-c-brand-1: #4aad35;
        --vp-c-brand-2: #5CC244;
        --vp-c-brand-3: #6ed058;
        --vp-c-brand-soft: rgba(92, 194, 68, 0.14);
        --vp-home-hero-name-color: transparent;
        --vp-home-hero-name-background: linear-gradient(135deg, #5CC244 0%, #D4FC37 100%);
        --vp-home-hero-image-background-image: linear-gradient(135deg, rgba(92, 194, 68, 0.3) 0%, rgba(212, 252, 55, 0.2) 100%);
        --vp-home-hero-image-filter: blur(56px);
      }
      .dark {
        --vp-c-brand-1: #6ed058;
        --vp-c-brand-2: #5CC244;
        --vp-c-brand-3: #4aad35;
        --vp-c-brand-soft: rgba(92, 194, 68, 0.16);
      }
      :root {
        --vp-button-brand-bg: #5CC244;
        --vp-button-brand-hover-bg: #4aad35;
        --vp-button-brand-active-bg: #3d9a2c;
      }
      likec4-view {
        display: block;
        width: 100%;
        height: 450px;
        margin: 1.5rem 0;
        border: 1px solid var(--vp-c-divider);
        border-radius: 8px;
        overflow: hidden;
      }
    `],
  ],
  themeConfig: {
    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Protocol', link: '/protocol/overview' },
      { text: 'Architecture', link: '/architecture/' },
    ],
    sidebar: {
      '/protocol/': [
        {
          text: 'RSCP Protocol',
          items: [
            { text: 'Overview', link: '/protocol/overview' },
            { text: 'Frame Format', link: '/protocol/frame-format' },
            { text: 'Data Types', link: '/protocol/data-types' },
            { text: 'Encryption', link: '/protocol/encryption' },
            { text: 'Authentication', link: '/protocol/authentication' },
          ]
        },
        {
          text: 'Tag Reference',
          items: [
            { text: 'Overview', link: '/protocol/tags/' },
            { text: 'EMS (Energy)', link: '/protocol/tags/ems' },
            { text: 'PVI (Inverter)', link: '/protocol/tags/pvi' },
            { text: 'BAT (Battery)', link: '/protocol/tags/bat' },
            { text: 'PM (Power Meter)', link: '/protocol/tags/pm' },
            { text: 'DB (History)', link: '/protocol/tags/db' },
            { text: 'WB (Wallbox)', link: '/protocol/tags/wb' },
            { text: 'INFO (Device)', link: '/protocol/tags/info' },
            { text: 'EP (Emergency)', link: '/protocol/tags/ep' },
          ]
        }
      ],
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Imperative Client', link: '/guide/imperative-client' },
            { text: 'Streaming', link: '/guide/streaming' },
            { text: 'Actor Integration', link: '/guide/actor-integration' },
            { text: 'Polling', link: '/guide/polling' },
            { text: 'Typed Snapshots', link: '/guide/typed-snapshots' },
            { text: 'Configuration', link: '/guide/configuration' },
          ]
        }
      ],
      '/architecture/': [
        {
          text: 'Architecture',
          items: [
            { text: 'Overview', link: '/architecture/' },
          ]
        }
      ]
    },
    search: { provider: 'local' },
    footer: {
      message: 'E3DC S10 Pro RSCP Client Library',
    },
  }
})
