import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'E3DC Connector',
  description: 'Akka.Streams RSCP client for E3DC S10 Pro',
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
  }
})
