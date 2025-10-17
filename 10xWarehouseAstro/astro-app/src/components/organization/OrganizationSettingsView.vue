<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-foreground">Organization Settings</h1>
            <p class="mt-2 text-sm text-muted-foreground">
              Manage your organization members and invitations
            </p>
          </div>
        </div>
      </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="flex items-center space-x-2">
        <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
        <span class="text-muted-foreground">Loading organization settings...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="rounded-md bg-destructive/10 p-4 border border-destructive/20">
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-destructive" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-destructive">Error loading organization settings</h3>
          <div class="mt-2 text-sm text-destructive/80">
            <p>{{ error }}</p>
          </div>
          <div class="mt-4">
            <button
              @click="loadOrganizationData"
              class="bg-destructive/10 px-2 py-1.5 rounded-md text-sm font-medium text-destructive hover:bg-destructive/20 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-destructive"
            >
              Try again
            </button>
          </div>
        </div>
      </div>
    </div>

      <!-- Main Content -->
      <div v-else class="space-y-8">
        <!-- Organization Info Section -->
        <OrganizationInfoSection
          :organization="organization"
          :loading="loading"
          :error="error"
          @update-organization="handleUpdateOrganization"
        />

        <!-- Members Section -->
        <MembersSection
          :members="members"
          :pagination="membersPagination"
          :loading="loading"
          :error="error"
          @remove-member="handleRemoveMember"
          @invite-user="handleInviteUser"
          @page-change="handleMembersPageChange"
        />

        <!-- Invitations Section -->
        <InvitationsSection
          :invitations="invitations"
          :pagination="invitationsPagination"
          :loading="loading"
          :error="error"
          @cancel-invitation="handleCancelInvitation"
          @invite-user="handleInviteUser"
          @page-change="handleInvitationsPageChange"
        />
      </div>

      <!-- Invite User Modal -->
      <InviteUserModal
        :is-open="isInviteModalOpen"
        :loading="inviteLoading"
        :error="inviteError"
        @close="handleCloseInviteModal"
        @submit="handleSubmitInvitation"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { organizationApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';
import { useAuthStore } from '@/stores/auth';
import type { OrganizationSettingsState, CreateInvitationRequestDto, UpdateOrganizationRequestDto } from '@/types/dto';
import OrganizationInfoSection from './OrganizationInfoSection.vue';
import MembersSection from './MembersSection.vue';
import InvitationsSection from './InvitationsSection.vue';
import InviteUserModal from './InviteUserModal.vue';

const organizationStore = useOrganizationStore();
const authStore = useAuthStore();

const state = ref<OrganizationSettingsState>({
  organization: null,
  members: [],
  invitations: [],
  loading: false,
  error: null,
  membersPagination: { page: 1, pageSize: 10, total: 0 },
  invitationsPagination: { page: 1, pageSize: 10, total: 0 },
  currentMembersPage: 1,
  currentInvitationsPage: 1,
  pageSize: 10,
  isInviteModalOpen: false,
  inviteLoading: false,
  inviteError: null
});

// Use computed properties to access reactive state
const organization = computed(() => state.value.organization);
const members = computed(() => state.value.members);
const invitations = computed(() => state.value.invitations);
const loading = computed(() => state.value.loading);
const error = computed(() => state.value.error);
const membersPagination = computed(() => state.value.membersPagination);
const invitationsPagination = computed(() => state.value.invitationsPagination);
const isInviteModalOpen = computed(() => state.value.isInviteModalOpen);
const inviteLoading = computed(() => state.value.inviteLoading);
const inviteError = computed(() => state.value.inviteError);

const currentOrganizationId = computed(() => organizationStore.currentOrganizationId);

const loadOrganizationData = async () => {
  if (!currentOrganizationId.value) return;
  
  state.value.loading = true;
  state.value.error = null;
  
  try {
    // Load organization info
    const orgResponse = await organizationApi.getOrganizations();
    const currentOrg = orgResponse.data.find(org => org.id === currentOrganizationId.value);
    state.value.organization = currentOrg || null;
    
    // Load members
    await loadMembers();
    
    // Load invitations
    await loadInvitations();
  } catch (err: any) {
    state.value.error = err.message || 'Failed to load organization data';
  } finally {
    state.value.loading = false;
  }
};

const loadMembers = async () => {
  if (!currentOrganizationId.value) return;
  
  try {
    const response = await organizationApi.getOrganizationMembers(
      currentOrganizationId.value,
      state.value.currentMembersPage,
      state.value.pageSize
    );
    state.value.members = response.data;
    state.value.membersPagination = response.pagination;
  } catch (err: any) {
    state.value.error = err.message || 'Failed to load members';
  }
};

const loadInvitations = async () => {
  if (!currentOrganizationId.value) return;
  
  try {
    // Note: This would need to be implemented in the backend
    // For now, we'll set empty data
    state.value.invitations = [];
    state.value.invitationsPagination = { page: 1, pageSize: 10, total: 0 };
  } catch (err: any) {
    state.value.error = err.message || 'Failed to load invitations';
  }
};

const handleUpdateOrganization = async (data: UpdateOrganizationRequestDto) => {
  if (!currentOrganizationId.value) return;
  
  try {
    const updatedOrg = await organizationApi.updateOrganization(currentOrganizationId.value, data);
    state.value.organization = updatedOrg;
  } catch (err: any) {
    state.value.error = err.message || 'Failed to update organization';
  }
};

const handleRemoveMember = async (userId: string) => {
  if (!currentOrganizationId.value) return;
  
  try {
    await organizationApi.removeOrganizationMember(currentOrganizationId.value, userId);
    await loadMembers(); // Refresh members list
  } catch (err: any) {
    state.value.error = err.message || 'Failed to remove member';
  }
};

const handleCancelInvitation = async (invitationId: string) => {
  if (!currentOrganizationId.value) return;
  
  try {
    await organizationApi.cancelInvitation(currentOrganizationId.value, invitationId);
    await loadInvitations(); // Refresh invitations list
  } catch (err: any) {
    state.value.error = err.message || 'Failed to cancel invitation';
  }
};

const handleInviteUser = () => {
  state.value.isInviteModalOpen = true;
  state.value.inviteError = null;
};

const handleCloseInviteModal = () => {
  state.value.isInviteModalOpen = false;
  state.value.inviteError = null;
};

const handleSubmitInvitation = async (data: CreateInvitationRequestDto) => {
  if (!currentOrganizationId.value) return;
  
  state.value.inviteLoading = true;
  state.value.inviteError = null;
  
  try {
    await organizationApi.createInvitation(currentOrganizationId.value, data);
    state.value.isInviteModalOpen = false;
    await loadInvitations(); // Refresh invitations list
  } catch (err: any) {
    state.value.inviteError = err.message || 'Failed to create invitation';
  } finally {
    state.value.inviteLoading = false;
  }
};

const handleMembersPageChange = (page: number) => {
  state.value.currentMembersPage = page;
  loadMembers();
};

const handleInvitationsPageChange = (page: number) => {
  state.value.currentInvitationsPage = page;
  loadInvitations();
};

onMounted(() => {
  loadOrganizationData();
});
</script>
