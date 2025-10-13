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

  const currentOrganization = computed(() => {
    return organizations.value.find(o => o.id === currentOrganizationId.value) || null;
  });

  const currentRole = computed(() => {
    if (!user.value || !currentOrganizationId.value) return null;
    const membership = user.value.organizations.find(org => org.id === currentOrganizationId.value);
    // This is a simplified version. In a real app, UserVM might need to be structured differently
    // or we'd find the role from a different property. For now, this is a placeholder.
    // Let's assume the user object holds role info per organization.
    return user.value.currentRole; // Placeholder
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

    try {
      const userData = await api.getUserData();
      const organizationsVM = userData.memberships.map(m => ({ id: m.organizationId, name: m.organizationName }));
      
      user.value = {
        id: userData.id,
        email: userData.email,
        displayName: userData.displayName,
        organizations: organizationsVM,
        currentOrganizationId: organizationsVM[0]?.id,
        currentRole: userData.memberships[0]?.role,
      };
      organizations.value = organizationsVM;
      if (organizationsVM.length > 0) {
        currentOrganizationId.value = organizationsVM[0].id;
      }
    } catch (error) {
      console.error('Failed to fetch user data:', error);
      // If auth error, sign out user
      if (error instanceof Error && error.message === 'Authentication required') {
        await authStore.signOut();
      }
    }
  }

  function switchOrganization(organizationId: string) {
    currentOrganizationId.value = organizationId;
    console.log(`Switched to organization ${organizationId}`);
  }

  async function createOrganization(data: CreateOrganizationRequestDto) {
    const newOrganization = await api.createOrganization(data);
    addOrganization(newOrganization);
    return newOrganization;
  }

  async function logout() {
    await authStore.signOut();
    user.value = null;
    organizations.value = [];
    currentOrganizationId.value = null;
    console.log('User logged out');
    // Redirect will be handled by the component calling this function
  }


  return {
    user,
    organizations,
    currentOrganizationId,
    currentOrganization,
    currentRole,
    addOrganization,
    fetchUserData,
    switchOrganization,
    createOrganization,
    logout,
  };
});
