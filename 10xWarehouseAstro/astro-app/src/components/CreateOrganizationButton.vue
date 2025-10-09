<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/button/Button.vue';

const uiStore = useUiStore();
</script>

<template>
  <Button @click="uiStore.openCreateOrganizationModal()">
    Create Organization
  </Button>
</template>
