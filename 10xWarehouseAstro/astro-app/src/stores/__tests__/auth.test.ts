import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useAuthStore } from '../auth';
import type { User } from '@supabase/supabase-js';

// Hoisted mocks to avoid initialization issues
const mockSignUp = vi.hoisted(() => vi.fn());
const mockSignIn = vi.hoisted(() => vi.fn());
const mockSignOut = vi.hoisted(() => vi.fn());
const mockGetSession = vi.hoisted(() => vi.fn());
const mockOnAuthStateChange = vi.hoisted(() => vi.fn());

vi.mock('@/lib/supabase', () => ({
  supabase: {
    auth: {
      signUp: mockSignUp,
      signInWithPassword: mockSignIn,
      signOut: mockSignOut,
      getSession: mockGetSession,
      onAuthStateChange: mockOnAuthStateChange,
    },
  },
}));

// Mock API
const mockRegister = vi.hoisted(() => vi.fn());
vi.mock('@/lib/api', () => ({
  api: {
    register: mockRegister,
  },
}));

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    
    // Setup default mock implementations
    mockSignOut.mockResolvedValue({ error: null });
    mockGetSession.mockResolvedValue({
      data: { session: null },
      error: null,
    });
    mockOnAuthStateChange.mockReturnValue({
      data: { subscription: null },
      unsubscribe: vi.fn(),
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('State Management', () => {
    it('should initialize with null user and session', () => {
      const store = useAuthStore();
      
      expect(store.user).toBeNull();
      expect(store.session).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.isInitialized).toBe(false);
      expect(store.isAuthenticated).toBe(false);
    });

    it('should compute isAuthenticated correctly', () => {
      const store = useAuthStore();
      
      expect(store.isAuthenticated).toBe(false);
      
      const mockUser = { id: '123', email: 'test@example.com' } as User;
      store.user = mockUser;
      
      expect(store.isAuthenticated).toBe(true);
    });
  });

  describe('Token Management', () => {
    it('should get access token from session', () => {
      const store = useAuthStore();
      const mockSession = {
        access_token: 'test-token-123',
        user: null,
      };
      
      store.session = mockSession as any;
      
      expect(store.getAccessToken()).toBe('test-token-123');
    });

    it('should return undefined when no session exists', () => {
      const store = useAuthStore();
      
      expect(store.getAccessToken()).toBeUndefined();
    });

    it('should return undefined when session has no token', () => {
      const store = useAuthStore();
      store.session = { user: null } as any;
      
      expect(store.getAccessToken()).toBeUndefined();
    });
  });

  describe('signUp', () => {
    it('should successfully sign up a user', async () => {
      const store = useAuthStore();
      const mockUser = { id: '123', email: 'test@example.com' } as User;
      const mockData = { user: mockUser, session: null };
      
      mockSignUp.mockResolvedValue({ data: mockData, error: null });
      
      const result = await store.signUp('test@example.com', 'password123', 'Test User');
      
      expect(mockSignUp).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        options: {
          data: {
            display_name: 'Test User',
          },
        },
      });
      expect(result.data).toEqual(mockData);
      expect(result.error).toBeNull();
      expect(store.loading).toBe(false);
    });

    it('should handle sign up errors', async () => {
      const store = useAuthStore();
      const mockError = new Error('Email already exists');
      
      mockSignUp.mockResolvedValue({ data: null, error: mockError });
      
      const result = await store.signUp('test@example.com', 'password123');
      
      expect(result.data).toBeNull();
      expect(result.error).toEqual(mockError);
      expect(store.loading).toBe(false);
    });

    it('should set loading state during sign up', async () => {
      const store = useAuthStore();
      let resolvePromise: (value: any) => void;
      const promise = new Promise((resolve) => {
        resolvePromise = resolve;
      });
      
      mockSignUp.mockReturnValue(promise);
      
      const signUpPromise = store.signUp('test@example.com', 'password123');
      
      expect(store.loading).toBe(true);
      
      resolvePromise!({ data: null, error: null });
      await signUpPromise;
      
      expect(store.loading).toBe(false);
    });

    it('should sign up without display name', async () => {
      const store = useAuthStore();
      mockSignUp.mockResolvedValue({ data: null, error: null });
      
      await store.signUp('test@example.com', 'password123');
      
      expect(mockSignUp).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        options: {
          data: {
            display_name: undefined,
          },
        },
      });
    });
  });

  describe('registerWithBackend', () => {
    it('should successfully register with backend', async () => {
      const store = useAuthStore();
      const mockData = {
        user: { id: '123', email: 'test@example.com' },
        organization: { id: 'org-1', name: 'Test Org' },
      };
      
      mockRegister.mockResolvedValue(mockData);
      
      const result = await store.registerWithBackend(
        'test@example.com',
        'password123',
        'Test User',
        true,
        'Test Org'
      );
      
      expect(mockRegister).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        displayName: 'Test User',
        createOrganization: true,
        organizationName: 'Test Org',
      });
      expect(result.data).toEqual(mockData);
      expect(result.error).toBeNull();
      expect(store.loading).toBe(false);
    });

    it('should handle registration errors', async () => {
      const store = useAuthStore();
      const mockError = new Error('Registration failed');
      
      mockRegister.mockRejectedValue(mockError);
      
      const result = await store.registerWithBackend(
        'test@example.com',
        'password123',
        'Test User',
        false
      );
      
      expect(result.data).toBeNull();
      expect(result.error).toEqual(mockError);
      expect(store.loading).toBe(false);
    });
  });

  describe('signIn', () => {
    it('should successfully sign in a user', async () => {
      const store = useAuthStore();
      const mockUser = { id: '123', email: 'test@example.com' } as User;
      const mockSession = {
        user: mockUser,
        access_token: 'token-123',
      };
      const mockData = { user: mockUser, session: mockSession };
      
      mockSignIn.mockResolvedValue({ data: mockData, error: null });
      
      const result = await store.signIn('test@example.com', 'password123');
      
      expect(mockSignIn).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
      expect(result.data).toEqual(mockData);
      expect(result.error).toBeNull();
      expect(store.loading).toBe(false);
    });

    it('should handle sign in errors', async () => {
      const store = useAuthStore();
      const mockError = new Error('Invalid credentials');
      
      mockSignIn.mockResolvedValue({ data: null, error: mockError });
      
      const result = await store.signIn('test@example.com', 'wrongpassword');
      
      expect(result.data).toBeNull();
      expect(result.error).toEqual(mockError);
      expect(store.loading).toBe(false);
    });

    it('should set loading state during sign in', async () => {
      const store = useAuthStore();
      let resolvePromise: (value: any) => void;
      const promise = new Promise((resolve) => {
        resolvePromise = resolve;
      });
      
      mockSignIn.mockReturnValue(promise);
      
      const signInPromise = store.signIn('test@example.com', 'password123');
      
      expect(store.loading).toBe(true);
      
      resolvePromise!({ data: null, error: null });
      await signInPromise;
      
      expect(store.loading).toBe(false);
    });
  });

  describe('signOut', () => {
    it('should successfully sign out a user', async () => {
      const store = useAuthStore();
      const mockUser = { id: '123', email: 'test@example.com' } as User;
      store.user = mockUser;
      store.session = { user: mockUser } as any;
      
      mockSignOut.mockResolvedValue({ error: null });
      
      await store.signOut();
      
      expect(mockSignOut).toHaveBeenCalled();
      expect(store.user).toBeNull();
      expect(store.session).toBeNull();
      expect(store.loading).toBe(false);
    });

    it('should handle sign out errors gracefully', async () => {
      const store = useAuthStore();
      const mockError = new Error('Sign out failed');
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      mockSignOut.mockResolvedValue({ error: mockError });
      
      await store.signOut();
      
      expect(consoleErrorSpy).toHaveBeenCalledWith('Error signing out:', mockError);
      expect(store.loading).toBe(false);
      
      consoleErrorSpy.mockRestore();
    });
  });

  describe('clearSession', () => {
    it('should clear session and set password change flag', async () => {
      const store = useAuthStore();
      store.user = { id: '123' } as User;
      store.session = { user: null } as any;
      
      await store.clearSession();
      
      expect(store.user).toBeNull();
      expect(store.session).toBeNull();
      expect(store.isPasswordChangeInProgress).toBe(true);
      expect(store.loading).toBe(false);
    });

    it('should call signOut with local scope', async () => {
      const store = useAuthStore();
      
      await store.clearSession();
      
      expect(mockSignOut).toHaveBeenCalledWith({ scope: 'local' });
    });

    it('should clear password change flag when requested', () => {
      const store = useAuthStore();
      store.isPasswordChangeInProgress = true;
      
      store.clearPasswordChangeFlag();
      
      expect(store.isPasswordChangeInProgress).toBe(false);
    });
  });

  describe('initializeAuth', () => {
    it('should initialize auth with existing session', async () => {
      const store = useAuthStore();
      const mockUser = { id: '123', email: 'test@example.com' } as User;
      const mockSession = {
        user: mockUser,
        access_token: 'token-123',
      };
      
      mockGetSession.mockResolvedValue({
        data: { session: mockSession },
        error: null,
      });
      
      await store.initializeAuth();
      
      expect(mockGetSession).toHaveBeenCalled();
      expect(store.session).toEqual(mockSession);
      expect(store.user).toEqual(mockUser);
      expect(store.isInitialized).toBe(true);
      expect(mockOnAuthStateChange).toHaveBeenCalled();
    });

    it('should initialize auth without session', async () => {
      const store = useAuthStore();
      
      mockGetSession.mockResolvedValue({
        data: { session: null },
        error: null,
      });
      
      await store.initializeAuth();
      
      expect(store.session).toBeNull();
      expect(store.user).toBeNull();
      expect(store.isInitialized).toBe(true);
    });

    it('should not initialize twice', async () => {
      const store = useAuthStore();
      
      mockGetSession.mockResolvedValue({
        data: { session: null },
        error: null,
      });
      
      await store.initializeAuth();
      expect(mockGetSession).toHaveBeenCalledTimes(1);
      
      await store.initializeAuth();
      expect(mockGetSession).toHaveBeenCalledTimes(1);
    });

    it('should handle initialization errors', async () => {
      const store = useAuthStore();
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const mockError = new Error('Initialization failed');
      
      mockGetSession.mockRejectedValue(mockError);
      
      await store.initializeAuth();
      
      expect(consoleErrorSpy).toHaveBeenCalledWith('Error initializing auth:', mockError);
      expect(store.isInitialized).toBe(false);
      
      consoleErrorSpy.mockRestore();
    });

    it('should set up auth state change listener', async () => {
      const store = useAuthStore();
      let authChangeCallback: (event: string, session: any) => void;
      
      mockGetSession.mockResolvedValue({
        data: { session: null },
        error: null,
      });
      
      mockOnAuthStateChange.mockImplementation((callback) => {
        authChangeCallback = callback;
        return {
          data: { subscription: null },
          unsubscribe: vi.fn(),
        };
      });
      
      await store.initializeAuth();
      
      // Simulate auth state change
      const newUser = { id: '456', email: 'new@example.com' } as User;
      const newSession = { user: newUser, access_token: 'new-token' };
      authChangeCallback!('SIGNED_IN', newSession);
      
      expect(store.user).toEqual(newUser);
      expect(store.session).toEqual(newSession);
    });

    it('should handle SIGNED_OUT event', async () => {
      const store = useAuthStore();
      let authChangeCallback: (event: string, session: any) => void;
      
      mockGetSession.mockResolvedValue({
        data: { session: null },
        error: null,
      });
      
      mockOnAuthStateChange.mockImplementation((callback) => {
        authChangeCallback = callback;
        return {
          data: { subscription: null },
          unsubscribe: vi.fn(),
        };
      });
      
      await store.initializeAuth();
      
      // Simulate sign out
      store.user = { id: '123' } as User;
      store.session = { user: null } as any;
      authChangeCallback!('SIGNED_OUT', null);
      
      expect(store.user).toBeNull();
      expect(store.session).toBeNull();
    });
  });
});

