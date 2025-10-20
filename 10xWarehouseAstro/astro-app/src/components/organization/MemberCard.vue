<template>
  <div class="bg-card rounded-lg border border-border p-6 hover:shadow-md transition-shadow">
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <h3 class="text-lg font-semibold text-foreground mb-2">
          {{ member.userDisplayName || member.email || 'Unknown User' }}
        </h3>
        
        <p v-if="member.userDisplayName && member.email" class="text-sm text-muted-foreground mb-2">
          {{ member.email }}
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
      
      <div v-if="canRemove" class="flex items-center space-x-2 ml-4">
        <Button
          variant="outline"
          size="sm"
          @click="handleRemove"
          :disabled="loading"
          :aria-label="`Remove member ${member.userDisplayName || member.email || 'user'}`"
          class="text-destructive hover:text-destructive/80 hover:bg-destructive/10"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { MemberCardProps } from '@/types/dto';
import { Button } from '@/components/ui/button';

const props = defineProps<MemberCardProps>();

const emit = defineEmits<{
  remove: [userId: string];
}>();

const roleDisplay = computed(() => {
  switch (props.member.role) {
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
  switch (props.member.status) {
    case 'Accepted':
      return {
        label: 'Active',
        color: 'bg-primary/10 text-primary'
      };
    case 'Pending':
      return {
        label: 'Pending',
        color: 'bg-accent/10 text-accent-foreground'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-muted text-muted-foreground'
      };
  }
});

const canRemove = computed(() => {
  // Can't remove self or if no userId (pending invitation)
  return props.member.userId && props.member.userId !== props.currentUserId;
});

const handleRemove = () => {
  if (props.member.userId) {
    emit('remove', props.member.userId);
  }
};
</script>
