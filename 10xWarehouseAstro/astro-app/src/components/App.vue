<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import SidebarNav from './SidebarNav.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';

const authStore = useAuthStore();
const router = useRouter();
const isInitializing = ref(true);

onMounted(async () => {
  // Wait for auth to be initialized if it hasn't been yet
  if (!authStore.isInitialized) {
    await authStore.initializeAuth();
  }
  
  // Redirect to login if not authenticated (only for protected routes)
  const currentPath = window.location.pathname;
  const publicRoutes = ['/login', '/register'];
  if (!authStore.isAuthenticated && !publicRoutes.includes(currentPath)) {
    window.location.href = '/login';
    return;
  }
  
  isInitializing.value = false;
});

// Watch for auth state changes and redirect if logged out
watch(() => authStore.isAuthenticated, (isAuthenticated) => {
  if (!isInitializing.value) {
    const currentPath = window.location.pathname;
    const publicRoutes = ['/login', '/register'];
    
    if (!isAuthenticated && !publicRoutes.includes(currentPath)) {
      window.location.href = '/login';
    } else if (isAuthenticated && publicRoutes.includes(currentPath)) {
      router.push('/');
    }
  }
});
</script>

<template>
  <div v-if="isInitializing" class="flex items-center justify-center min-h-screen">
    <div class="text-lg">Loading...</div>
  </div>
  <div v-else-if="authStore.isAuthenticated" class="flex min-h-screen" data-allow-mismatch>
    <SidebarNav />
    <main class="flex-1 overflow-auto">
      <router-view />
    </main>
    <CreateOrganizationModal />
  </div>
  <div v-else class="h-screen">
    <main class="h-full">
      <slot></slot>
    </main>
  </div>
</template>
