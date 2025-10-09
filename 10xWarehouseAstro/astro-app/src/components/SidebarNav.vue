<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useOrganizationStore } from '@/stores/organization';
import { useUiStore } from '@/stores/ui';
import NavItem from './NavItem.vue';
import OrganizationSwitcher from './OrganizationSwitcher.vue';
import CreateOrganizationButton from './CreateOrganizationButton.vue';
import CreateOrganizationModal from './CreateOrganizationModal.vue';
import type { NavItemVM } from '@/types/dto';
import { Button } from '@/components/ui/button';

const organizationStore = useOrganizationStore();
const uiStore = useUiStore();

onMounted(() => {
  organizationStore.fetchUserData();
});

const navItems = computed<NavItemVM[]>(() => {
  const items: NavItemVM[] = [
    { to: '/', label: 'Dashboard', icon: '' },
    { to: '/inventory', label: 'Inventory', icon: '' },
    { to: '/warehouses', label: 'Warehouses', icon: '' },
    { to: '/products', label: 'Products', icon: '' },
    { to: '/movements', label: 'Stock Movements', icon: '' },
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
</script>

<template>
  <aside class="h-screen w-64 bg-gray-50 dark:bg-gray-800 flex flex-col">
    <div class="p-4">
      <OrganizationSwitcher
        :organizations="organizationStore.organizations"
        :current-organization-id="organizationStore.currentOrganizationId"
        @select="handleOrgSwitch"
      />
      <div class="mt-2">
        <CreateOrganizationButton />
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
    </nav>
    
    <div class="p-4 mt-auto">
      <Button @click="organizationStore.logout()" variant="outline" class="w-full">
        Logout
      </Button>
    </div>
  </aside>
</template>
