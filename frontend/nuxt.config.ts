import tailwindcss from "@tailwindcss/vite";

export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  css: ['~/assets/css/tailwind.css'],
  modules: ['@nuxt/eslint', '@nuxt/fonts', 'nuxt-svgo'],
  vite: {
    plugins: [tailwindcss()],
  },
  svgo: {
    autoImportPath: '~/assets/svg/',
    defaultImport: 'component',
  },
})