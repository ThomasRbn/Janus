import tailwindcss from "@tailwindcss/vite";

export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  css: ['~/assets/css/tailwind.css'],
  modules: ['@nuxt/eslint', '@nuxt/fonts', '@nuxt/icon'],
  vite: {
    plugins: [tailwindcss()],
  },
})