<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useOrganizationStore } from '@/stores/organization';
import Button from '@/components/ui/button/Button.vue';

const uiStore = useUiStore();
const organizationStore = useOrganizationStore();
</script>

<template>
  <Button 
    @click="uiStore.openCreateOrganizationModal()"
    :disabled="organizationStore.loading"
  >
    {{ organizationStore.loading ? 'Loading...' : 'Create Organization' }}
  </Button>
</template>
