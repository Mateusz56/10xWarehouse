import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type {
  OrganizationDto,
  OrganizationMemberDto,
  InvitationDto,
  PaginationDto,
  CreateInvitationRequestDto,
  UpdateOrganizationRequestDto
} from '@/types/dto';
import { organizationApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useOrganizationSettingsStore = defineStore('organizationSettings', () => {
  const orgStore = useOrganizationStore();

  // State
  const organization = ref<OrganizationDto | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const members = ref<OrganizationMemberDto[]>([]);
  const membersPagination = ref<PaginationDto>({ page: 1, pageSize: 10, total: 0 });
  const membersCurrentPage = ref(1);
  const membersPageSize = ref(10);

  const invitations = ref<InvitationDto[]>([]);
  const invitationsPagination = ref<PaginationDto>({ page: 1, pageSize: 10, total: 0 });
  const invitationsCurrentPage = ref(1);
  const invitationsPageSize = ref(10);

  // Computed
  const hasMembers = computed(() => members.value.length > 0);
  const hasInvitations = computed(() => invitations.value.length > 0);

  // Actions
  async function fetchOrganization(): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const response = await organizationApi.getOrganizations();
      organization.value = response.data.find(o => o.id === orgStore.currentOrganizationId) || null;
    } catch (err) {
      console.error('Failed to fetch organization:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load organization';
    } finally {
      loading.value = false;
    }
  }

  async function fetchMembers(page?: number, size?: number): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const targetPage = page ?? membersCurrentPage.value;
      const targetPageSize = size ?? membersPageSize.value;

      if (targetPage < 1) {
        throw new Error('Page number must be greater than 0');
      }
      if (targetPageSize < 1 || targetPageSize > 100) {
        throw new Error('Page size must be between 1 and 100');
      }

      const response = await organizationApi.getOrganizationMembers(
        orgStore.currentOrganizationId,
        targetPage,
        targetPageSize
      );

      members.value = response.data;
      membersPagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };

      membersCurrentPage.value = targetPage;
      membersPageSize.value = targetPageSize;
    } catch (err) {
      console.error('Failed to fetch members:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load members';
    } finally {
      loading.value = false;
    }
  }

  async function fetchInvitations(page?: number, size?: number): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const targetPage = page ?? invitationsCurrentPage.value;
      const targetPageSize = size ?? invitationsPageSize.value;

      if (targetPage < 1) {
        throw new Error('Page number must be greater than 0');
      }
      if (targetPageSize < 1 || targetPageSize > 100) {
        throw new Error('Page size must be between 1 and 100');
      }

      const response = await organizationApi.getOrganizationInvitations(
        orgStore.currentOrganizationId,
        targetPage,
        targetPageSize
      );

      invitations.value = response.data;
      invitationsPagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };

      invitationsCurrentPage.value = targetPage;
      invitationsPageSize.value = targetPageSize;
    } catch (err) {
      console.error('Failed to fetch invitations:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load invitations';
    } finally {
      loading.value = false;
    }
  }

  async function updateOrganization(request: UpdateOrganizationRequestDto): Promise<OrganizationDto> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      const result = await organizationApi.updateOrganization(orgStore.currentOrganizationId, request);
      organization.value = result;
      return result;
    } catch (err) {
      console.error('Failed to update organization:', err);
      error.value = err instanceof Error ? err.message : 'Failed to update organization';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function removeMember(userId: string): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      await organizationApi.removeOrganizationMember(orgStore.currentOrganizationId, userId);
      await fetchMembers(membersCurrentPage.value, membersPageSize.value);
    } catch (err) {
      console.error('Failed to remove member:', err);
      error.value = err instanceof Error ? err.message : 'Failed to remove member';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function cancelInvitation(invitationId: string): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      await organizationApi.cancelInvitation(orgStore.currentOrganizationId, invitationId);
      await fetchInvitations(invitationsCurrentPage.value, invitationsPageSize.value);
    } catch (err) {
      console.error('Failed to cancel invitation:', err);
      error.value = err instanceof Error ? err.message : 'Failed to cancel invitation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function createInvitation(request: CreateInvitationRequestDto): Promise<void> {
    // Ensure organization store is initialized first
    if (!orgStore.user) {
      await orgStore.fetchUserData();
    }

    if (!orgStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      await organizationApi.createInvitation(orgStore.currentOrganizationId, request);
      await fetchInvitations(1, invitationsPageSize.value);
    } catch (err) {
      console.error('Failed to create invitation:', err);
      error.value = err instanceof Error ? err.message : 'Failed to create invitation';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function setMembersPage(page: number): void {
    membersCurrentPage.value = page;
  }

  function setInvitationsPage(page: number): void {
    invitationsCurrentPage.value = page;
  }

  function clearError(): void {
    error.value = null;
  }

  return {
    // State
    organization,
    loading,
    error,
    members,
    membersPagination,
    membersCurrentPage,
    membersPageSize,
    invitations,
    invitationsPagination,
    invitationsCurrentPage,
    invitationsPageSize,

    // Computed
    hasMembers,
    hasInvitations,

    // Actions
    fetchOrganization,
    fetchMembers,
    fetchInvitations,
    updateOrganization,
    removeMember,
    cancelInvitation,
    createInvitation,
    setMembersPage,
    setInvitationsPage,
    clearError,
  };
});


