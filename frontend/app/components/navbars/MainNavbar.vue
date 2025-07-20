<template>
  <nav class="bg-jbackground-900/95 backdrop-blur-lg border-b border-jprimary-800/30 sticky top-0 z-50">
    <div class="px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16">

        <!-- Logo -->
        <div class="flex-shrink-0">
          <NuxtLink to="/" class="flex items-center space-x-2 group">
            <SvgoJanusLogo
              class="w-8 h-8 text-jprimary-300 group-hover:text-jprimary-400 transition-colors duration-200" />
            <span
              class="text-jtext-100 font-semibold text-xl group-hover:text-jprimary-300 transition-colors duration-200">
              Janus
            </span>
          </NuxtLink>
        </div>

        <div class="flex-shrink-0">
          <button
            class="bg-gradient-to-r from-jprimary-500 to-jaccent-500 hover:from-jprimary-400 hover:to-jaccent-400 text-jtext-100 px-6 py-2 rounded-lg font-semibold">
            Sign up
          </button>
          <button
            class="ml-4 bg-jprimary-800 hover:bg-jprimary-700 text-jtext-100 px-6 py-2 rounded-lg font-semibold">
            Log in
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>