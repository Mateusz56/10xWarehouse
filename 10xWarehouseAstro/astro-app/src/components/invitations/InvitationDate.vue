<template>
  <div class="flex items-center text-sm text-muted-foreground">
    <svg class="flex-shrink-0 mr-1.5 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
    <span>Invited {{ formattedDate }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  invitedAt: string;
}

const props = defineProps<Props>();

const formattedDate = computed(() => {
  const date = new Date(props.invitedAt);
  const now = new Date();
  const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
  
  if (diffInDays === 0) {
    return 'today';
  } else if (diffInDays === 1) {
    return 'yesterday';
  } else if (diffInDays < 7) {
    return `${diffInDays} days ago`;
  } else {
    return date.toLocaleDateString();
  }
});
</script>
