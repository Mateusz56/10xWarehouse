<template>
  <div>
    <!-- Empty State -->
    <div v-if="invitations.length === 0 && !loading" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-foreground">No invitations</h3>
      <p class="mt-1 text-sm text-muted-foreground">
        You don't have any pending invitations at the moment.
      </p>
    </div>

    <!-- Invitations Grid -->
    <div v-if="invitations.length > 0" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <InvitationCard
        v-for="invitation in invitations"
        :key="invitation.id"
        :invitation="invitation"
        :loading="processingInvitations.has(invitation.id)"
        :disabled="processingInvitations.size > 0"
        @accept="$emit('accept', invitation.id)"
        @decline="$emit('decline', invitation.id)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { UserInvitationDto } from '@/types/dto';
import InvitationCard from './InvitationCard.vue';

interface Props {
  invitations: UserInvitationDto[];
  loading: boolean;
  error: string | null;
  processingInvitations: Set<string>;
}

defineProps<Props>();

defineEmits<{
  accept: [invitationId: string];
  decline: [invitationId: string];
}>();
</script>
