<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-foreground">My Invitations</h1>
            <p class="mt-2 text-sm text-muted-foreground">
              Manage your pending invitations to join organizations
            </p>
          </div>
        </div>
      </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="flex items-center space-x-2">
        <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
        <span class="text-muted-foreground">Loading invitations...</span>
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
          <h3 class="text-sm font-medium text-destructive">Error loading invitations</h3>
          <div class="mt-2 text-sm text-destructive/80">
            <p>{{ error }}</p>
          </div>
          <div class="mt-4">
            <button
              @click="loadInvitations"
              class="bg-destructive/10 px-2 py-1.5 rounded-md text-sm font-medium text-destructive hover:bg-destructive/20 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-destructive"
            >
              Try again
            </button>
          </div>
        </div>
      </div>
    </div>


      <!-- Invitations List -->
      <InvitationsList
        v-else
        :invitations="invitations"
        :loading="loading"
        :error="error"
        :processing-invitations="processingInvitations"
        @accept="handleAcceptInvitation"
        @decline="handleDeclineInvitation"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { invitationsApi } from '@/lib/api';
import type { UserInvitationDto, MyInvitationsState } from '@/types/dto';
import InvitationsList from './InvitationsList.vue';

const state = ref<MyInvitationsState>({
  invitations: [],
  loading: false,
  error: null,
  processingInvitations: new Set<string>()
});

// Use computed properties to access reactive state
const invitations = computed(() => state.value.invitations);
const loading = computed(() => state.value.loading);
const error = computed(() => state.value.error);
const processingInvitations = computed(() => state.value.processingInvitations);

const loadInvitations = async () => {
  state.value.loading = true;
  state.value.error = null;
  
  try {
    const response = await invitationsApi.getUserInvitations();
    state.value.invitations = response.data;
  } catch (err: any) {
    state.value.error = err.message || 'Failed to load invitations';
  } finally {
    state.value.loading = false;
  }
};

const handleAcceptInvitation = async (invitationId: string) => {
  state.value.processingInvitations.add(invitationId);
  
  try {
    await invitationsApi.acceptInvitation(invitationId);
    // Remove the invitation from the list
    state.value.invitations = state.value.invitations.filter(
      inv => inv.id !== invitationId
    );
  } catch (err: any) {
    state.value.error = err.message || 'Failed to accept invitation';
  } finally {
    state.value.processingInvitations.delete(invitationId);
  }
};

const handleDeclineInvitation = async (invitationId: string) => {
  state.value.processingInvitations.add(invitationId);
  
  try {
    await invitationsApi.declineInvitation(invitationId);
    // Remove the invitation from the list
    state.value.invitations = state.value.invitations.filter(
      inv => inv.id !== invitationId
    );
  } catch (err: any) {
    state.value.error = err.message || 'Failed to decline invitation';
  } finally {
    state.value.processingInvitations.delete(invitationId);
  }
};

onMounted(() => {
  loadInvitations();
});
</script>
