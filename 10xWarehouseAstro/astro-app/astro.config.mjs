// @ts-check

import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'astro/config';

import vue from '@astrojs/vue';

// https://astro.build/config
export default defineConfig({
  output: 'server', // Enable server-side rendering for dynamic routes
  
  vite: {
      plugins: [tailwindcss()],
	},

  integrations: [
    vue({
      appEntrypoint: '/src/pages/_app'
    })
  ],
});