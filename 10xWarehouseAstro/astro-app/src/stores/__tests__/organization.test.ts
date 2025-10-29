import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useOrganizationStore } from '../organization';
import type { OrganizationDto, UserDto } from '@/types/dto';

// Mock auth store
const mockAuthStore = {
  isAuthenticated: false,
  signOut: vi.fn(),
};

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => mockAuthStore,
}));

// Hoisted mocks to avoid initialization issues
const mockGetUserData = vi.hoisted(() => vi.fn());
const mockCreateOrganization = vi.hoisted(() => vi.fn());

// Mock API
vi.mock('@/lib/api', () => ({
  api: {
    getUserData: mockGetUserData,
    createOrganization: mockCreateOrganization,
  },
}));

describe('organizationStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    
    // Reset mock auth store
    mockAuthStore.isAuthenticated = false;
    mockAuthStore.signOut.mockResolvedValue(undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('State Management', () => {
    it('should initialize with empty state', () => {
      const store = useOrganizationStore();
      
      expect(store.user).toBeNull();
      expect(store.organizations).toEqual([]);
      expect(store.currentOrganizationId).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    });

    it('should compute currentOrganization correctly', () => {
      const store = useOrganizationStore();
      const org1 = { id: 'org-1', name: 'Organization 1' };
      const org2 = { id: 'org-2', name: 'Organization 2' };
      
      store.organizations = [org1, org2];
      store.currentOrganizationId = 'org-1';
      
      expect(store.currentOrganization).toEqual(org1);
    });

    it('should return null when currentOrganizationId does not exist', () => {
      const store = useOrganizationStore();
      store.organizations = [{ id: 'org-1', name: 'Org 1' }];
      store.currentOrganizationId = 'non-existent';
      
      expect(store.currentOrganization).toBeNull();
    });

    it('should compute currentRole correctly from user memberships', () => {
      const store = useOrganizationStore();
      store.currentOrganizationId = 'org-1';
      store.user = {
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Test User',
        organizations: [],
        currentOrganizationId: 'org-1',
        currentRole: 'Owner',
        memberships: [
          { organizationId: 'org-1', role: 'Owner' },
          { organizationId: 'org-2', role: 'Member' },
        ],
      };
      
      expect(store.currentRole).toBe('Owner');
    });

    it('should return null for currentRole when no user or organization', () => {
      const store = useOrganizationStore();
      
      expect(store.currentRole).toBeNull();
      
      store.currentOrganizationId = 'org-1';
      expect(store.currentRole).toBeNull();
      
      store.currentOrganizationId = null;
      store.user = { id: 'user-1' } as any;
      expect(store.currentRole).toBeNull();
    });
  });

  describe('addOrganization', () => {
    it('should add organization to list', () => {
      const store = useOrganizationStore();
      const newOrg: OrganizationDto = {
        id: 'org-1',
        name: 'New Organization',
      };
      
      store.addOrganization(newOrg);
      
      expect(store.organizations).toHaveLength(1);
      expect(store.organizations[0]).toEqual({
        id: 'org-1',
        name: 'New Organization',
      });
    });

    it('should switch to new organization after adding', () => {
      const store = useOrganizationStore();
      const newOrg: OrganizationDto = {
        id: 'org-1',
        name: 'New Organization',
      };
      
      store.addOrganization(newOrg);
      
      expect(store.currentOrganizationId).toBe('org-1');
    });

    it('should handle multiple organizations', () => {
      const store = useOrganizationStore();
      const org1: OrganizationDto = { id: 'org-1', name: 'Org 1' };
      const org2: OrganizationDto = { id: 'org-2', name: 'Org 2' };
      
      store.addOrganization(org1);
      store.addOrganization(org2);
      
      expect(store.organizations).toHaveLength(2);
      expect(store.currentOrganizationId).toBe('org-2');
    });
  });

  describe('fetchUserData', () => {
    it('should not fetch when user is not authenticated', async () => {
      const store = useOrganizationStore();
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      mockAuthStore.isAuthenticated = false;
      
      await store.fetchUserData();
      
      expect(mockGetUserData).not.toHaveBeenCalled();
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        'User not authenticated, cannot fetch user data'
      );
      
      consoleWarnSpy.mockRestore();
    });

    it('should set loading state during fetch', async () => {
      const store = useOrganizationStore();
      mockAuthStore.isAuthenticated = true;
      
      let resolvePromise: (value: UserDto) => void;
      const promise = new Promise<UserDto>((resolve) => {
        resolvePromise = resolve;
      });
      
      mockGetUserData.mockReturnValue(promise);
      
      const fetchPromise = store.fetchUserData();
      
      expect(store.loading).toBe(true);
      
      resolvePromise!({
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Test',
        memberships: [],
      });
      await fetchPromise;
      
      expect(store.loading).toBe(false);
    });

    it('should handle fetch errors', async () => {
      const store = useOrganizationStore();
      mockAuthStore.isAuthenticated = true;
      const mockError = new Error('Network error');
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      mockGetUserData.mockRejectedValue(mockError);
      
      await store.fetchUserData();
      
      expect(store.error).toBe('Network error');
      expect(store.loading).toBe(false);
      expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to fetch user data:', mockError);
      
      consoleErrorSpy.mockRestore();
    });

    it('should sign out on authentication error', async () => {
      const store = useOrganizationStore();
      mockAuthStore.isAuthenticated = true;
      const authError = new Error('Authentication required');
      
      mockGetUserData.mockRejectedValue(authError);
      
      await store.fetchUserData();
      
      expect(mockAuthStore.signOut).toHaveBeenCalled();
      expect(store.error).toBe('Authentication required');
    });

    it('should handle empty organizations list', async () => {
      const store = useOrganizationStore();
      mockAuthStore.isAuthenticated = true;
      
      const mockUserData: UserDto = {
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Test User',
        memberships: [],
      };
      
      mockGetUserData.mockResolvedValue(mockUserData);
      
      await store.fetchUserData();
      
      expect(store.organizations).toEqual([]);
      expect(store.currentOrganizationId).toBeNull();
    });
  });

  describe('switchOrganization', () => {
    it('should switch to different organization', () => {
      const store = useOrganizationStore();
      store.currentOrganizationId = 'org-1';
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
      
      store.switchOrganization('org-2');
      
      expect(store.currentOrganizationId).toBe('org-2');
      expect(consoleLogSpy).toHaveBeenCalledWith('Switched to organization org-2');
      
      consoleLogSpy.mockRestore();
    });

    it('should allow switching to same organization', () => {
      const store = useOrganizationStore();
      store.currentOrganizationId = 'org-1';
      
      store.switchOrganization('org-1');
      
      expect(store.currentOrganizationId).toBe('org-1');
    });
  });

  describe('createOrganization', () => {
    it('should successfully create organization', async () => {
      const store = useOrganizationStore();
      const newOrg: OrganizationDto = {
        id: 'org-1',
        name: 'New Organization',
      };
      
      mockCreateOrganization.mockResolvedValue(newOrg);
      
      const result = await store.createOrganization({ name: 'New Organization' });
      
      expect(mockCreateOrganization).toHaveBeenCalledWith({ name: 'New Organization' });
      expect(result).toEqual(newOrg);
      expect(store.organizations).toHaveLength(1);
      expect(store.currentOrganizationId).toBe('org-1');
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    });

    it('should handle creation errors', async () => {
      const store = useOrganizationStore();
      const mockError = new Error('Creation failed');
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      mockCreateOrganization.mockRejectedValue(mockError);
      
      await expect(store.createOrganization({ name: 'New Org' })).rejects.toThrow(mockError);
      
      expect(store.error).toBe('Creation failed');
      expect(store.loading).toBe(false);
      expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to create organization:', mockError);
      
      consoleErrorSpy.mockRestore();
    });

    it('should set loading state during creation', async () => {
      const store = useOrganizationStore();
      
      let resolvePromise: (value: OrganizationDto) => void;
      const promise = new Promise<OrganizationDto>((resolve) => {
        resolvePromise = resolve;
      });
      
      mockCreateOrganization.mockReturnValue(promise);
      
      const createPromise = store.createOrganization({ name: 'New Org' });
      
      expect(store.loading).toBe(true);
      
      resolvePromise!({ id: 'org-1', name: 'New Org' });
      await createPromise;
      
      expect(store.loading).toBe(false);
    });
  });

  describe('logout', () => {
    it('should sign out and clear credential state', async () => {
      const store = useOrganizationStore();
      store.user = { id: 'user-1' } as any;
      store.organizations = [{ id: 'org-1', name: 'Org 1' }];
      store.currentOrganizationId = 'org-1';
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
      
      await store.logout();
      
      expect(mockAuthStore.signOut).toHaveBeenCalled();
      expect(store.user).toBeNull();
      expect(store.organizations).toEqual([]);
      expect(store.currentOrganizationId).toBeNull();
      expect(consoleLogSpy).toHaveBeenCalledWith('User logged out');
      
      consoleLogSpy.mockRestore();
    });
  });

  describe('clearError', () => {
    it('should clear error state', () => {
      const store = useOrganizationStore();
      store.error = 'Some error';
      
      store.clearError();
      
      expect(store.error).toBeNull();
    });
  });
});

