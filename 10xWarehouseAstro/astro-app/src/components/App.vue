<script setup lang="ts">
import { ref, onMounted, watch, computed, nextTick, inject } from 'vue';
import { useRouter, RouterView } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import SidebarNav from './SidebarNav.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';

const authStore = useAuthStore();

// Check if router is available using inject first (doesn't throw if not available)
// Then try useRouter() which must be called unconditionally
const routerKey = Symbol.for('router');
let router: ReturnType<typeof useRouter> | undefined;
const routerAvailable = ref(false);

// Try inject first to check if router exists without throwing
const injectedRouter = inject(routerKey, null);
if (injectedRouter) {
  router = injectedRouter as ReturnType<typeof useRouter>;
  routerAvailable.value = true;
} else {
  // If inject didn't find it, try useRouter() (required to be called unconditionally)
  try {
    router = useRouter();
    routerAvailable.value = true;
  } catch (error) {
    // Router not available yet - this is expected during SSR
    // It will be available on client-side after _app.ts installs it
    if (typeof window !== 'undefined') {
      console.debug('Router not available during setup, will initialize in onMounted');
    }
  }
}

// Check if router is available and fully initialized
const hasRouter = computed(() => {
  return routerAvailable.value && !!router && !!router.currentRoute;
});

// Create a dynamic RouterView component that only renders when router is ready
const RouterViewComponent = computed(() => {
  if (hasRouter.value) {
    return RouterView;
  }
  return null;
});

const isInitializing = ref(true);

onMounted(async () => {
  // Ensure we're on client-side
  if (typeof window === 'undefined') return;

  // Try to get router if we don't have it yet, or verify it's ready
  if (!router || !routerAvailable.value) {
    try {
      router = useRouter();
      // Verify router is fully initialized
      if (router && router.currentRoute) {
        routerAvailable.value = true;
      }
    } catch (error) {
      // Wait for router to be available - it should be set up by _app.ts
      await nextTick();
      // Try a few times with small delays
      for (let i = 0; i < 3; i++) {
        try {
          router = useRouter();
          if (router && router.currentRoute) {
            routerAvailable.value = true;
            break;
          }
        } catch (retryError) {
          if (i === 2) {
            console.error('Router failed to initialize after multiple retries:', retryError);
          } else {
            await new Promise(resolve => setTimeout(resolve, 50));
          }
        }
      }
    }
  } else {
    // Verify router is still ready
    routerAvailable.value = !!(router && router.currentRoute);
  }

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
  if (!isInitializing.value && typeof window !== 'undefined') {
    const currentPath = window.location.pathname;
    const publicRoutes = ['/login', '/register'];
    
    if (!isAuthenticated && !publicRoutes.includes(currentPath)) {
      window.location.href = '/login';
    } else if (isAuthenticated && publicRoutes.includes(currentPath) && router) {
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
      <component :is="RouterViewComponent" v-if="hasRouter" />
      <div v-else class="flex items-center justify-center min-h-full">
        <div class="text-lg">Loading router...</div>
      </div>
    </main>
    <CreateOrganizationModal />
  </div>
  <div v-else class="h-screen">
    <main class="h-full">
      <slot></slot>
    </main>
  </div>
</template>
