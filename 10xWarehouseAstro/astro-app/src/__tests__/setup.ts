import { vi } from 'vitest';

// Global mocks and test setup
// This file runs before all tests

// Mock Supabase client for tests
vi.mock('@/lib/supabase', () => ({
  supabase: {
    auth: {
      getSession: vi.fn(),
      signInWithPassword: vi.fn(),
      signUp: vi.fn(),
      signOut: vi.fn(),
      getUser: vi.fn(),
      onAuthStateChange: vi.fn(() => ({
        data: { subscription: null },
        unsubscribe: vi.fn(),
      })),
    },
  },
}));

// Mock API calls
vi.mock('@/lib/api', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

// Setup global test utilities
global.console = {
  ...console,
  // Suppress console warnings in tests if needed
  warn: vi.fn(),
  error: vi.fn(),
};

