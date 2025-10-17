<template>
  <div class="flex items-center space-x-2">
    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="roleDisplay.color">
      {{ roleDisplay.label }}
    </span>
    <span class="text-sm text-muted-foreground">{{ roleDisplay.description }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { UserRole, RoleDisplay } from '@/types/dto';

interface Props {
  role: UserRole;
}

const props = defineProps<Props>();

const roleDisplay = computed<RoleDisplay>(() => {
  switch (props.role) {
    case 'Owner':
      return {
        label: 'Owner',
        color: 'bg-primary/10 text-primary',
        description: 'Full access to organization'
      };
    case 'Member':
      return {
        label: 'Member',
        color: 'bg-secondary/10 text-secondary-foreground',
        description: 'Can manage inventory and products'
      };
    case 'Viewer':
      return {
        label: 'Viewer',
        color: 'bg-accent/10 text-accent-foreground',
        description: 'Read-only access'
      };
    default:
      return {
        label: 'Unknown',
        color: 'bg-muted text-muted-foreground',
        description: 'Unknown role'
      };
  }
});
</script>
