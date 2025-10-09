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
        'flex items-center p-2 text-gray-900 rounded-lg dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 group',
        { 'bg-gray-200 dark:bg-gray-700': isActive }
      ]"
    >
      <!-- Icon will go here -->
      <span class="ms-3">{{ item.label }}</span>
    </a>
  </li>
</template>
