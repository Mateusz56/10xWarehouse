<script setup lang="ts">
import type { NavItemVM } from '@/types/dto';

defineProps<{
  item: NavItemVM;
  isActive: boolean;
}>();
</script>

<template>
  <li>
    <a 
      :href="item.to" 
      :class="[
        'flex items-center p-2 text-sidebar-foreground rounded-lg hover:bg-sidebar-accent hover:text-sidebar-accent-foreground group',
        { 'bg-sidebar-primary text-sidebar-primary-foreground': isActive }
      ]"
    >
      <!-- Icon will go here -->
      <span class="ms-3">{{ item.label }}</span>
    </a>
  </li>
</template>
