import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { OrganizationDto, OrganizationVM, UserVM, CreateOrganizationRequestDto } from '@/types/dto';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/auth';

export const useOrganizationStore = defineStore('organization', () => {
  const authStore = useAuthStore();
  const user = ref<UserVM | null>(null);
  const organizations = ref<OrganizationVM[]>([]);
  const currentOrganizationId = ref<string | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const currentOrganization = computed(() => {
    return organizations.value.find(o => o.id === currentOrganizationId.value) || null;
  });

  const currentRole = computed(() => {
    if (!user.value || !currentOrganizationId.value) return null;
    // Find the membership for the current organization
    const membership = user.value.memberships?.find(m => m.organizationId === currentOrganizationId.value);
    return membership?.role || null;
  });

  function addOrganization(newOrganization: OrganizationDto) {
    organizations.value.push({
      id: newOrganization.id,
      name: newOrganization.name,
    });
    // Optionally switch to the new organization
    currentOrganizationId.value = newOrganization.id;
  }
  
  async function fetchUserData() {
    if (!authStore.isAuthenticated) {
      console.warn('User not authenticated, cannot fetch user data');
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const userData = await api.getUserData();
      const organizationsVM = userData.memberships.map(m => ({ id: m.organizationId, name: m.organizationName }));
      
      user.value = {
        id: userData.id,
        email: userData.email,
        displayName: userData.displayName,
        organizations: organizationsVM,
        currentOrganizationId: organizationsVM[0]?.id || '',
        currentRole: userData.memberships[0]?.role || 'Viewer',
        memberships: userData.memberships,
      };
      organizations.value = organizationsVM;
      if (organizationsVM.length > 0) {
        currentOrganizationId.value = organizationsVM[0].id;
      }
    } catch (err) {
      console.error('Failed to fetch user data:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load user data';
      
      // If auth error, sign out user
      if (err instanceof Error && err.message === 'Authentication required') {
        await authStore.signOut();
      }
    } finally {
      loading.value = false;
    }
  }

  function switchOrganization(organizationId: string) {
    currentOrganizationId.value = organizationId;
    console.log(`Switched to organization ${organizationId}`);
  }

  async function createOrganization(data: CreateOrganizationRequestDto) {
    loading.value = true;
    error.value = null;

    try {
      const newOrganization = await api.createOrganization(data);
      addOrganization(newOrganization);
      return newOrganization;
    } catch (err) {
      console.error('Failed to create organization:', err);
      error.value = err instanceof Error ? err.message : 'Failed to create organization';
      throw err; // Re-throw so the component can handle it
    } finally {
      loading.value = false;
    }
  }

  async function logout() {
    await authStore.signOut();
    user.value = null;
    organizations.value = [];
    currentOrganizationId.value = null;
    console.log('User logged out');
    // Redirect will be handled by the component calling this function
  }


  function clearError() {
    error.value = null;
  }

  return {
    user,
    organizations,
    currentOrganizationId,
    currentOrganization,
    currentRole,
    loading,
    error,
    addOrganization,
    fetchUserData,
    switchOrganization,
    createOrganization,
    logout,
    clearError,
  };
});
