import DefaultTheme from 'vitepress/theme'
import type { Theme } from 'vitepress'

export default {
  extends: DefaultTheme,
  enhanceApp({ app, router, siteData }) {
    if (typeof window !== 'undefined') {
      import('./likec4-views.js').catch(() => {})
    }
  },
} satisfies Theme
