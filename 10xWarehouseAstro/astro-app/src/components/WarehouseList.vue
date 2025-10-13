<script setup lang="ts">
import { computed } from 'vue';
import { useWarehouseStore } from '@/stores/warehouse';
import { useUiStore } from '@/stores/ui';
import WarehouseCard from './WarehouseCard.vue';
import type { WarehouseVM } from '@/types/dto';

const props = defineProps<{
  warehouses: WarehouseVM[];
  loading: boolean;
}>();

const warehouseStore = useWarehouseStore();
const uiStore = useUiStore();

const isEmpty = computed(() => !props.loading && props.warehouses.length === 0);
</script>

<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="n in 6" 
        :key="n"
        class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6 animate-pulse"
      >
        <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded mb-4"></div>
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="isEmpty" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No warehouses</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
        Get started by creating your first warehouse.
      </p>
      <div v-if="warehouseStore.canCreateWarehouse" class="mt-6">
        <button
          @click="uiStore.openCreateWarehouseModal()"
          class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Create Warehouse
        </button>
      </div>
    </div>

    <!-- Warehouse Cards -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6" role="grid" aria-label="Warehouse list">
      <div
        v-for="warehouse in warehouses"
        :key="warehouse.id"
        role="gridcell"
      >
        <WarehouseCard :warehouse="warehouse" />
      </div>
    </div>
  </div>
</template>
