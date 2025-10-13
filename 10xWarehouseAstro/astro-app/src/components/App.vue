<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import SidebarNav from './SidebarNav.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';

const authStore = useAuthStore();
const isInitializing = ref(true);

onMounted(async () => {
  // Wait for auth to be initialized if it hasn't been yet
  if (!authStore.isInitialized) {
    await authStore.initializeAuth();
  }
  
  isInitializing.value = false;
});
</script>

<template>
  <div v-if="isInitializing" class="flex items-center justify-center min-h-screen">
    <div class="text-lg">Loading...</div>
  </div>
  <div v-else-if="authStore.isAuthenticated" class="flex h-screen">
    <aside class="w-64 bg-gray-100 dark:bg-gray-800">
      <SidebarNav />
    </aside>
    <main class="flex-1">
      <slot></slot>
    </main>
    <CreateOrganizationModal />
  </div>
  <div v-else class="h-screen">
    <main class="h-full">
      <slot></slot>
    </main>
  </div>
</template>
