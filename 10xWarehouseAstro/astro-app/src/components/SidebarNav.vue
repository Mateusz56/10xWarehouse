<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useOrganizationStore } from '@/stores/organization';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import NavItem from './NavItem.vue';
import OrganizationSwitcher from './OrganizationSwitcher.vue';
import CreateOrganizationButton from './CreateOrganizationButton.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';
import type { NavItemVM } from '@/types/dto';
import { Button } from '@/components/ui/button';

const organizationStore = useOrganizationStore();
const authStore = useAuthStore();
const uiStore = useUiStore();

onMounted(async () => {
  // Initialize theme
  uiStore.initializeTheme();
  
  // Wait for auth to be initialized if it hasn't been yet
  if (!authStore.isInitialized) {
    await authStore.initializeAuth();
  }
  
  if (authStore.isAuthenticated) {
    organizationStore.fetchUserData();
  }
});

const navItems = computed<NavItemVM[]>(() => {
  const items: NavItemVM[] = [
    { to: '/', label: 'Dashboard', icon: '' },
    { to: '/inventory', label: 'Inventory', icon: '' },
    { to: '/warehouses', label: 'Warehouses', icon: '' },
    { to: '/products', label: 'Products', icon: '' },
    { to: '/movements', label: 'Stock Movements', icon: '' },
    { to: '/invitations', label: 'My Invitations', icon: '' },
    { to: '/profile', label: 'Profile', icon: '' },
    { to: '/settings/organization', label: 'Settings', icon: '', requiredRole: 'Owner' },
  ];

  return items.filter(item => {
    if (!item.requiredRole) return true;
    return organizationStore.currentRole === item.requiredRole;
  });
});

const currentPage = '/'; // This would come from Astro's currentPage prop

function handleOrgSwitch(orgId: string) {
  organizationStore.switchOrganization(orgId);
}

async function handleLogout() {
  await organizationStore.logout();
  window.location.href = '/login';
}
</script>

<template>
  <aside class="h-screen w-64 bg-sidebar text-sidebar-foreground flex flex-col sticky top-0 z-50">
    <div class="p-4">
      <OrganizationSwitcher
        :organizations="organizationStore.organizations"
        :current-organization-id="organizationStore.currentOrganizationId"
        :loading="organizationStore.loading"
        @select="handleOrgSwitch"
      />
      <div class="mt-2">
        <CreateOrganizationButton />
      </div>
      
      <!-- Error display -->
      <div v-if="organizationStore.error" class="mt-3 p-2 bg-red-50 border border-red-200 rounded-md">
        <p class="text-sm text-red-600">{{ organizationStore.error }}</p>
        <button 
          @click="organizationStore.clearError" 
          class="text-xs text-red-500 hover:text-red-700 mt-1"
        >
          Dismiss
        </button>
      </div>
    </div>

    <nav class="flex-grow px-3 py-4 overflow-y-auto">
      <ul class="space-y-2 font-medium">
        <NavItem
          v-for="item in navItems"
          :key="item.to"
          :item="item"
          :is-active="currentPage === item.to"
        />
      </ul>
      
      <!-- Theme Toggle Button -->
      <div class="mt-4 px-2">
        <Button 
          @click="uiStore.toggleTheme" 
          variant="outline" 
          size="sm"
          class="w-full justify-start"
          :aria-label="uiStore.isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
        >
          <svg v-if="uiStore.isDarkMode" class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
          </svg>
          <svg v-else class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
          </svg>
          {{ uiStore.isDarkMode ? 'Dark Mode' : 'Light Mode' }}
        </Button>
      </div>
    </nav>
    
    <div class="p-4 mt-auto">
      <Button 
        v-if="authStore.isAuthenticated"
        @click="handleLogout" 
        variant="outline" 
        class="w-full"
        :disabled="authStore.loading"
      >
        {{ authStore.loading ? 'Logging out...' : 'Logout' }}
      </Button>
    </div>
  </aside>
</template>
