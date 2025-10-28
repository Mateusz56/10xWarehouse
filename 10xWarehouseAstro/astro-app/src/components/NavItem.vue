<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import type { NavItemVM } from '@/types/dto';

const props = defineProps<{
  item: NavItemVM;
  isActive: boolean;
}>();

const route = useRoute();
const isRouteActive = computed(() => route.path === props.item.to);
</script>

<template>
  <li>
    <router-link 
      :to="item.to" 
      :class="[
        'flex items-center p-2 text-sidebar-foreground rounded-lg hover:bg-sidebar-accent hover:text-sidebar-accent-foreground group',
        { 'bg-sidebar-primary text-sidebar-primary-foreground': isRouteActive }
      ]"
    >
      <!-- Icon will go here -->
      <span class="ms-3">{{ item.label }}</span>
    </router-link>
  </li>
</template>
