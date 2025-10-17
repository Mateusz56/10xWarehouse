<template>
  <div class="flex space-x-3">
    <button
      @click="$emit('accept', invitation.id)"
      :disabled="loading || disabled"
      class="flex-1 bg-primary text-primary-foreground px-4 py-2 rounded-md text-sm font-medium hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
    >
      <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-primary-foreground" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
      <svg v-else class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
      </svg>
      {{ loading ? 'Accepting...' : 'Accept' }}
    </button>
    
    <button
      @click="$emit('decline', invitation.id)"
      :disabled="loading || disabled"
      class="flex-1 bg-muted text-muted-foreground px-4 py-2 rounded-md text-sm font-medium hover:bg-muted/80 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
    >
      <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-muted-foreground" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
      <svg v-else class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
      {{ loading ? 'Declining...' : 'Decline' }}
    </button>
  </div>
</template>

<script setup lang="ts">
import type { UserInvitationDto } from '@/types/dto';

interface Props {
  invitation: UserInvitationDto;
  loading: boolean;
  disabled: boolean;
}

defineProps<Props>();

defineEmits<{
  accept: [invitationId: string];
  decline: [invitationId: string];
}>();
</script>
