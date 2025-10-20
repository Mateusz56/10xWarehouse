<template>
  <div class="bg-card rounded-lg border border-border p-6 hover:shadow-md transition-shadow">
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <h3 class="text-lg font-semibold text-foreground mb-2">
          {{ invitation.invitedUserDisplayName || invitation.invitedUserEmail || 'Unknown User' }}
        </h3>
        
        <p v-if="invitation.invitedUserEmail && invitation.invitedUserDisplayName" class="text-sm text-muted-foreground mb-2">
          {{ invitation.invitedUserEmail }}
        </p>
        
        <div class="flex items-center space-x-2">
          <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="roleDisplay.color">
            {{ roleDisplay.label }}
          </span>
          <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="statusDisplay.color">
            {{ statusDisplay.label }}
          </span>
        </div>
      </div>
      
      <div class="flex items-center space-x-2 ml-4">
        <Button
          variant="outline"
          size="sm"
          @click="handleCancel"
          :disabled="loading"
          :aria-label="`Cancel invitation for ${invitation.invitedUserDisplayName || invitation.invitedUserEmail || 'user'}`"
          class="text-destructive hover:text-destructive/80 hover:bg-destructive/10"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { InvitationDto } from '@/types/dto';
import { Button } from '@/components/ui/button';

interface Props {
  invitation: InvitationDto;
  loading: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  cancel: [invitationId: string];
}>();

const roleDisplay = computed(() => {
  switch (props.invitation.role) {
    case 'Owner':
      return {
        label: 'Owner',
        color: 'bg-primary/10 text-primary'
      };
    case 'Member':
      return {
        label: 'Member',
        color: 'bg-secondary/10 text-secondary-foreground'
      };
    case 'Viewer':
      return {
        label: 'Viewer',
        color: 'bg-accent/10 text-accent-foreground'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-muted text-muted-foreground'
      };
  }
});

const statusDisplay = computed(() => {
  switch (props.invitation.status) {
    case 'Pending':
      return {
        label: 'Pending',
        color: 'bg-accent/10 text-accent-foreground'
      };
    case 'Accepted':
      return {
        label: 'Accepted',
        color: 'bg-primary/10 text-primary'
      };
    case 'Declined':
      return {
        label: 'Declined',
        color: 'bg-destructive/10 text-destructive'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-muted text-muted-foreground'
      };
  }
});

const handleCancel = () => {
  emit('cancel', props.invitation.id);
};
</script>
