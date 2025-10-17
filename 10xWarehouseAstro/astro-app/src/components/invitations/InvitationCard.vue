<template>
  <div class="bg-card overflow-hidden shadow rounded-lg border border-border hover:shadow-md transition-shadow">
    <div class="p-6">
      <!-- Organization Info -->
      <div class="flex items-center space-x-3 mb-4">
        <div class="flex-shrink-0">
          <div class="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
            <svg class="h-6 w-6 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
          </div>
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="text-lg font-medium text-card-foreground truncate">
            {{ invitation.organizationName }}
          </h3>
          <p class="text-sm text-muted-foreground">Organization invitation</p>
        </div>
      </div>

      <!-- Role Info -->
      <div class="mb-4">
        <RoleInfo :role="invitation.role" />
      </div>

      <!-- Invitation Date -->
      <div class="mb-6">
        <InvitationDate :invited-at="invitation.invitedAt" />
      </div>

      <!-- Action Buttons -->
      <ActionButtons
        :invitation="invitation"
        :loading="loading"
        :disabled="disabled"
        @accept="$emit('accept', invitation.id)"
        @decline="$emit('decline', invitation.id)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { InvitationCardProps } from '@/types/dto';
import RoleInfo from './RoleInfo.vue';
import InvitationDate from './InvitationDate.vue';
import ActionButtons from './ActionButtons.vue';

defineProps<InvitationCardProps>();

defineEmits<{
  accept: [invitationId: string];
  decline: [invitationId: string];
}>();
</script>
