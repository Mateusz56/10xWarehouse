<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';
import { useRouter } from 'vue-router';
import type { WarehouseWithLocationsDto } from '@/types/dto';
import { Button } from '@/components/ui/button';

const props = defineProps<{
  warehouse: WarehouseWithLocationsDto;
}>();

const router = useRouter();
const uiStore = useUiStore();
const warehouseDetailsStore = useWarehouseDetailsStore();

function handleEditWarehouse() {
  uiStore.openEditWarehouseDetailsModal(props.warehouse);
}

function handleDeleteWarehouse() {
  uiStore.openDeleteWarehouseDetailsModal(props.warehouse);
}

function handleBackToWarehouses() {
  router.push('/warehouses');
}
</script>

<template>
  <div class="mb-8">
    <!-- Breadcrumb Navigation -->
    <nav class="flex items-center space-x-2 text-sm text-muted-foreground mb-6">
      <button 
        @click="handleBackToWarehouses"
        class="hover:text-foreground transition-colors"
      >
        Warehouses
      </button>
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
      </svg>
      <span class="text-foreground font-medium">{{ warehouse.name }}</span>
    </nav>

    <!-- Warehouse Header -->
    <div class="bg-card rounded-lg border border-border p-6">
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <div class="flex items-center space-x-4 mb-4">
            <div class="flex items-center justify-center w-12 h-12 bg-primary/10 rounded-lg">
              <svg class="w-6 h-6 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <div>
              <h1 class="text-2xl font-bold text-foreground">
                {{ warehouse.name }}
              </h1>
              <p class="text-sm text-muted-foreground">
                Warehouse ID: {{ warehouse.id.slice(0, 8) }}...
              </p>
            </div>
          </div>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="flex items-center text-sm text-muted-foreground">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>{{ warehouse.locations.length }} locations</span>
            </div>
            
            <div class="flex items-center text-sm text-muted-foreground">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
              <span>Organization: {{ warehouse.organizationId.slice(0, 8) }}...</span>
            </div>
            
            <div class="flex items-center text-sm text-muted-foreground">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Created recently</span>
            </div>
          </div>
        </div>
        
        <div 
          v-if="warehouseDetailsStore.canEditWarehouse || warehouseDetailsStore.canDeleteWarehouse"
          class="flex items-center space-x-3 ml-6"
        >
          <Button
            v-if="warehouseDetailsStore.canEditWarehouse"
            variant="outline"
            @click="handleEditWarehouse"
            :aria-label="`Edit warehouse ${warehouse.name}`"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
            Edit Warehouse
          </Button>
          
          <Button
            v-if="warehouseDetailsStore.canDeleteWarehouse"
            variant="outline"
            @click="handleDeleteWarehouse"
            :aria-label="`Delete warehouse ${warehouse.name}`"
            class="text-red-600 hover:text-red-700 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/20"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
            Delete Warehouse
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
