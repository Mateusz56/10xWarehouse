<script setup lang="ts">
import CreateOrganizationButton from './CreateOrganizationButton.vue';
import { useOrganizationStore } from '@/stores/organization';

const organizationStore = useOrganizationStore();
</script>

<template>
  <nav class="p-4">
    <CreateOrganizationButton />

    <div class="mt-8">
      <h2 class="text-xs font-semibold text-gray-500 uppercase tracking-wider">Organizations</h2>
      <ul>
        <li v-for="org in organizationStore.organizations" :key="org.id">
          {{ org.name }}
        </li>
        <li v-if="!organizationStore.organizations.length" class="text-sm text-gray-400 mt-2">
          No organizations yet.
        </li>
      </ul>
    </div>
  </nav>
</template>
