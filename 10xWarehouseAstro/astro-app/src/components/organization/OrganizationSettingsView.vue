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
import { onMounted, ref, computed } from 'vue';
import { useOrganizationStore } from '@/stores/organization';
import { useOrganizationSettingsStore } from '@/stores/organizationSettings';
import type { CreateInvitationRequestDto, UpdateOrganizationRequestDto } from '@/types/dto';
import OrganizationInfoSection from './OrganizationInfoSection.vue';
import MembersSection from './MembersSection.vue';
import InvitationsSection from './InvitationsSection.vue';
import InviteUserModal from './InviteUserModal.vue';

const organizationStore = useOrganizationStore();
const settingsStore = useOrganizationSettingsStore();

// Use computed properties to ensure reactivity
const organization = computed(() => settingsStore.organization);
const members = computed(() => settingsStore.members);
const invitations = computed(() => settingsStore.invitations);
const loading = computed(() => settingsStore.loading);
const error = computed(() => settingsStore.error);
const membersPagination = computed(() => settingsStore.membersPagination);
const invitationsPagination = computed(() => settingsStore.invitationsPagination);

// Local UI state
const isInviteModalOpen = ref(false);
const inviteLoading = ref(false);
const inviteError = ref<string | null>(null);

const loadOrganizationData = async () => {
  await settingsStore.fetchOrganization();
  await settingsStore.fetchMembers();
  await settingsStore.fetchInvitations();
};

const loadMembers = async () => settingsStore.fetchMembers();
const loadInvitations = async () => settingsStore.fetchInvitations();

const handleUpdateOrganization = async (data: UpdateOrganizationRequestDto) => {
  try {
    await settingsStore.updateOrganization(data);
  } catch (err: any) {
    inviteError.value = err.message || 'Failed to update organization';
  }
};

const handleRemoveMember = async (userId: string) => {
  try {
    await settingsStore.removeMember(userId);
  } catch (err: any) {
    inviteError.value = err.message || 'Failed to remove member';
  }
};

const handleCancelInvitation = async (invitationId: string) => {
  try {
    await settingsStore.cancelInvitation(invitationId);
  } catch (err: any) {
    inviteError.value = err.message || 'Failed to cancel invitation';
  }
};

const handleInviteUser = () => {
  isInviteModalOpen.value = true;
  inviteError.value = null;
};

const handleCloseInviteModal = () => {
  isInviteModalOpen.value = false;
  inviteError.value = null;
};

const handleSubmitInvitation = async (data: CreateInvitationRequestDto) => {
  inviteLoading.value = true;
  inviteError.value = null;
  try {
    await settingsStore.createInvitation(data);
    isInviteModalOpen.value = false;
  } catch (err: any) {
    inviteError.value = err.message || 'Failed to create invitation';
  } finally {
    inviteLoading.value = false;
  }
};

const handleMembersPageChange = (page: number) => {
  settingsStore.setMembersPage(page);
  settingsStore.fetchMembers(page);
};

const handleInvitationsPageChange = (page: number) => {
  settingsStore.setInvitationsPage(page);
  settingsStore.fetchInvitations(page);
};

onMounted(() => {
  loadOrganizationData();
});
</script>
