<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';
import type { LocationVM, PaginationDto } from '@/types/dto';
import { Button } from '@/components/ui/button';
import LocationList from './LocationList.vue';
import PaginationControl from './PaginationControl.vue';

const props = defineProps<{
  locations: LocationVM[];
  pagination: PaginationDto;
  loading: boolean;
}>();

const uiStore = useUiStore();
const warehouseDetailsStore = useWarehouseDetailsStore();

function handleCreateLocation() {
  uiStore.openCreateLocationModal();
}

function handlePageChange(page: number) {
  warehouseDetailsStore.setPage(page);
  const warehouseId = warehouseDetailsStore.warehouse?.id;
  if (warehouseId) {
    warehouseDetailsStore.fetchLocations(warehouseId, page, warehouseDetailsStore.pageSize);
  }
}

function handlePageSizeChange(pageSize: number) {
  warehouseDetailsStore.setPageSize(pageSize);
  const warehouseId = warehouseDetailsStore.warehouse?.id;
  if (warehouseId) {
    warehouseDetailsStore.fetchLocations(warehouseId, 1, pageSize);
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Locations Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-xl font-semibold text-foreground">
          Locations
        </h2>
        <p class="text-sm text-muted-foreground mt-1">
          Manage storage locations within this warehouse
        </p>
      </div>
      
      <Button
        v-if="warehouseDetailsStore.canCreateLocation"
        @click="handleCreateLocation"
        :aria-label="'Create new location'"
      >
        <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        Create Location
      </Button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="flex items-center space-x-2 text-muted-foreground">
        <svg class="animate-spin w-5 h-5" fill="none" viewBox="0 0 24 24" aria-hidden="true">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <span>Loading locations...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="warehouseDetailsStore.error" class="bg-destructive/10 border border-destructive/50 rounded-lg p-4">
      <div class="flex items-center">
        <svg class="w-5 h-5 text-destructive mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div>
          <h3 class="text-sm font-medium text-destructive">
            Error loading locations
          </h3>
          <p class="text-sm text-destructive mt-1">
            {{ warehouseDetailsStore.error }}
          </p>
        </div>
      </div>
    </div>

    <!-- Locations Content -->
    <div v-else>
      <LocationList :locations="locations" :loading="loading" />
      
      <!-- Pagination -->
      <div v-if="pagination.total > 0" class="mt-6">
        <PaginationControl
          :pagination="pagination"
          :onPageChange="handlePageChange"
          :onPageSizeChange="handlePageSizeChange"
        />
      </div>
    </div>
  </div>
</template>
