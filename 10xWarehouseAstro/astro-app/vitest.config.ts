import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath } from 'url';
import { resolve } from 'path';

export default defineConfig({
  plugins: [vue()],
  test: {
    // Use jsdom for DOM testing (needed for Vue components)
    environment: 'happy-dom',
    
    // Enable globals for easier test writing (expect, describe, it, etc.)
    globals: true,
    
    // Setup files to run before tests
    setupFiles: ['./src/__tests__/setup.ts'],
    
    // Include patterns
    include: ['src/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
    
    // Exclude patterns
    exclude: ['node_modules', 'dist', '.astro', 'e2e'],
    
    // Coverage configuration
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      exclude: [
        'node_modules/',
        'src/__tests__/',
        'src/**/*.d.ts',
        'src/**/*.config.{js,ts}',
        'dist/',
        '.astro/',
      ],
      // Set coverage thresholds (adjust as needed)
      thresholds: {
        lines: 70,
        functions: 70,
        branches: 65,
        statements: 70,
      },
    },
    
    // Test timeout in milliseconds
    testTimeout: 10000,
    
    // Enable watch mode optimizations
    watch: true,
    
    // UI mode configuration (disabled if package not installed)
    ui: false,
    
    // Type checking
    typecheck: {
      tsconfig: './tsconfig.json',
    },
  },
  resolve: {
    alias: {
      '@': resolve(fileURLToPath(new URL('.', import.meta.url)), './src'),
    },
  },
});

