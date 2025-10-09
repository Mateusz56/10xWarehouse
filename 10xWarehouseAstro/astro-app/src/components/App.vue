<script setup lang="ts">
import { getCurrentInstance } from 'vue';
import { pinia } from '@/stores';
import SidebarNav from './SidebarNav.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';

// Get the current Vue app instance and install Pinia
const vm = getCurrentInstance();
if (vm) {
  vm.appContext.app.use(pinia);
}
</script>

<template>
  <div class="flex h-screen">
    <aside class="w-64 bg-gray-100 dark:bg-gray-800">
      <SidebarNav />
    </aside>
    <main class="flex-1">
      <slot></slot>
    </main>
    <CreateOrganizationModal />
  </div>
</template>
