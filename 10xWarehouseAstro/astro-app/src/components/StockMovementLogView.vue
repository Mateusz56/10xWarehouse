<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold tracking-tight">Stock Movement Log</h1>
        <p class="text-muted-foreground">
          Complete audit trail of all inventory changes
        </p>
      </div>
    </div>

    <!-- Error State -->
    <div v-if="error" class="rounded-md border border-destructive/50 bg-destructive/10 p-4">
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-destructive" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-destructive">Error loading stock movements</h3>
          <div class="mt-2 text-sm text-destructive">
            <p>{{ error }}</p>
          </div>
          <div class="mt-4">
            <Button variant="outline" size="sm" @click="handleRetry">
              Try again
            </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- Data Grid -->
    <div v-else-if="!loading && hasMovements">
      <DataGrid
        :data="movements"
        :columns="columns"
        :loading="loading"
      >
        <template #rows="{ data }">
          <StockMovementRow
            v-for="movement in data"
            :key="movement.id"
            :movement="movement"
            :product="getProductById(movement.productTemplateId)"
            :from-location="getLocationById(movement.fromLocationId)"
            :to-location="getLocationById(movement.toLocationId)"
            :location="getLocationById(movement.fromLocationId || movement.toLocationId)"
            :quantity-before="calculateQuantityBefore(movement)"
            :quantity-after="calculateQuantityAfter(movement)"
          />
        </template>
      </DataGrid>

      <!-- Pagination -->
      <PaginationControl
        :pagination="pagination"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>

    <!-- Empty State -->
    <EmptyState
      v-else-if="!loading && !hasMovements"
      title="No stock movements found"
      message="There are no stock movements recorded for this organization yet."
    />

    <!-- Loading State -->
    <div v-else-if="loading" class="flex items-center justify-center p-8">
      <div class="flex items-center space-x-2">
        <div class="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
        <span class="text-sm text-muted-foreground">Loading stock movements...</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed, watch } from 'vue';
import { useStockMovementStore } from '@/stores/stockMovement';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import DataGrid from './ui/DataGrid.vue';
import PaginationControl from './ui/PaginationControl.vue';
import EmptyState from './ui/EmptyState.vue';
import { Button } from './ui/button';
import StockMovementRow from './StockMovementRow.vue';
import type { DataGridColumn } from '@/types/dto';

const stockMovementStore = useStockMovementStore();
const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

// Computed properties
const movements = computed(() => stockMovementStore.movements);
const loading = computed(() => stockMovementStore.loading);
const error = computed(() => stockMovementStore.error);
const pagination = computed(() => stockMovementStore.pagination);
const hasMovements = computed(() => stockMovementStore.hasMovements);

// Data grid columns
const columns: DataGridColumn[] = [
  { key: 'date', label: 'Date', width: '150px' },
  { key: 'product', label: 'Product', width: '200px' },
  { key: 'movementType', label: 'Type', width: '120px' },
  { key: 'delta', label: 'Quantity Change', width: '120px', align: 'right' },
  { key: 'locations', label: 'Location(s)', width: '200px' },
  { key: 'quantityBefore', label: 'Before', width: '100px', align: 'right' },
  { key: 'quantityAfter', label: 'After', width: '100px', align: 'right' }
];

// Methods
async function loadData() {
  // Wait for organization data to be loaded
  if (!organizationStore.currentOrganizationId) {
    console.log('Waiting for organization data to be loaded...');
    return;
  }
  
  await Promise.all([
    stockMovementStore.fetchMovements(),
    inventoryStore.fetchProducts(),
    inventoryStore.fetchLocations()
  ]);
}

function handlePageChange(page: number) {
  stockMovementStore.setPage(page);
  stockMovementStore.fetchMovements();
}

function handlePageSizeChange(pageSize: number) {
  stockMovementStore.setPageSize(pageSize);
  stockMovementStore.fetchMovements();
}

function handleRetry() {
  stockMovementStore.clearError();
  loadData();
}

function getProductById(id: string) {
  return inventoryStore.products.find(p => p.id === id) || { id, name: 'Unknown Product' };
}

function getLocationById(id?: string) {
  if (!id) return undefined;
  return inventoryStore.locations.find(l => l.id === id) || { id, name: 'Unknown Location' };
}

function calculateQuantityBefore(movement: any): number {
  return Math.max(0, movement.total - movement.delta);
}

function calculateQuantityAfter(movement: any): number {
  // after = total (the current quantity after the movement)
  return movement.total;
}

// Watchers
watch(
  () => organizationStore.currentOrganizationId,
  (newOrgId) => {
    if (newOrgId) {
      console.log('Organization loaded, fetching stock movement data...');
      loadData();
    }
  },
  { immediate: true }
);

// Lifecycle
onMounted(() => {
  // Try to load data immediately, but the watcher will handle it if org data isn't ready yet
  loadData();
});
</script>
