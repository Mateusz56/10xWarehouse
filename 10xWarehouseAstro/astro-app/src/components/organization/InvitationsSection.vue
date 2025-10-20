<template>
  <div class="bg-card shadow rounded-lg border border-border">
    <div class="px-4 py-5 sm:p-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg leading-6 font-medium text-card-foreground">
          Pending Invitations
        </h3>
        <button
          @click="$emit('invite-user')"
          class="bg-primary py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-primary-foreground hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring"
        >
          Invite User
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-8">
        <div class="flex items-center space-x-2">
          <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-primary"></div>
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
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="invitations.length === 0" class="text-center py-8">
        <svg class="mx-auto h-12 w-12 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-card-foreground">No pending invitations</h3>
        <p class="mt-1 text-sm text-muted-foreground">
          All invitations have been accepted or declined.
        </p>
        <div class="mt-6">
          <button
            @click="$emit('invite-user')"
            class="bg-primary py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-primary-foreground hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring"
          >
            Invite User
          </button>
        </div>
      </div>

      <!-- Invitations List -->
      <div v-if="invitations.length > 0" class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <InvitationCard
            v-for="invitation in invitations"
            :key="invitation.id"
            :invitation="invitation"
            :loading="false"
            @cancel="$emit('cancel-invitation', invitation.id)"
          />
        </div>

        <!-- Pagination -->
        <div class="px-6 py-4 border-t border-border">
          <PaginationControl
            v-if="pagination.total > pagination.pageSize"
            :current-page="pagination.page"
            :page-size="pagination.pageSize"
            :total="pagination.total"
            @page-change="$emit('page-change', $event)"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { InvitationDto, PaginationDto } from '@/types/dto';
import InvitationCard from './InvitationCard.vue';
import PaginationControl from '@/components/common/PaginationControl.vue';

interface Props {
  invitations: InvitationDto[];
  pagination: PaginationDto;
  loading: boolean;
  error: string | null;
}

defineProps<Props>();

defineEmits<{
  'cancel-invitation': [invitationId: string];
  'invite-user': [];
  'page-change': [page: number];
}>();
</script>
