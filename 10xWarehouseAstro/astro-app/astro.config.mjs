// @ts-check

import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'astro/config';

import vue from '@astrojs/vue';

import node from '@astrojs/node';

// https://astro.build/config
export default defineConfig({
  // Enable server-side rendering for dynamic routes
  output: 'server',

  vite: {
      plugins: [tailwindcss()],
    },

  integrations: [
    vue({
      appEntrypoint: '/src/pages/_app'
    })
  ],

  adapter: node({
    mode: 'standalone',
  }),
});