<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useWarehouseStore } from '@/stores/warehouse';
import { Button } from '@/components/ui/button';

const uiStore = useUiStore();
const warehouseStore = useWarehouseStore();
</script>

<template>
  <div class="flex items-center justify-between mb-6">
    <div>
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Warehouses</h1>
      <p class="text-gray-600 dark:text-gray-400 mt-1">
        Manage your organization's warehouses and locations
      </p>
    </div>
    
    <Button 
      v-if="warehouseStore.canCreateWarehouse"
      @click="uiStore.openCreateWarehouseModal()"
      :disabled="warehouseStore.loading"
    >
      <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
      </svg>
      Create Warehouse
    </Button>
  </div>
</template>
