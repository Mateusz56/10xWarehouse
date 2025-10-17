<template>
  <div class="bg-card border border-border rounded-lg p-4 hover:shadow-md transition-shadow">
    <div class="flex items-center justify-between">
      <div class="flex items-center space-x-3">
        <div class="flex-shrink-0">
          <div class="h-10 w-10 rounded-full bg-muted flex items-center justify-center">
            <svg class="h-6 w-6 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-card-foreground truncate">
            {{ member.email }}
          </p>
          <div class="flex items-center space-x-2 mt-1">
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="roleDisplay.color">
              {{ roleDisplay.label }}
            </span>
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="statusDisplay.color">
              {{ statusDisplay.label }}
            </span>
          </div>
        </div>
      </div>
      
      <div v-if="canRemove" class="flex-shrink-0">
        <button
          @click="handleRemove"
          :disabled="loading"
          class="text-destructive hover:text-destructive/80 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-destructive disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { MemberCardProps } from '@/types/dto';

const props = defineProps<MemberCardProps>();

const emit = defineEmits<{
  remove: [userId: string];
}>();

const roleDisplay = computed(() => {
  switch (props.member.role) {
    case 'Owner':
      return {
        label: 'Owner',
        color: 'bg-purple-100 text-purple-800'
      };
    case 'Member':
      return {
        label: 'Member',
        color: 'bg-blue-100 text-blue-800'
      };
    case 'Viewer':
      return {
        label: 'Viewer',
        color: 'bg-green-100 text-green-800'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-gray-100 text-gray-800'
      };
  }
});

const statusDisplay = computed(() => {
  switch (props.member.status) {
    case 'Accepted':
      return {
        label: 'Active',
        color: 'bg-green-100 text-green-800'
      };
    case 'Pending':
      return {
        label: 'Pending',
        color: 'bg-yellow-100 text-yellow-800'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-gray-100 text-gray-800'
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
