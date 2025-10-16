<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-foreground">Stock Movement Log</h1>
            <p class="mt-2 text-sm text-muted-foreground">
              Complete audit trail of all inventory changes
            </p>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-if="error" class="rounded-md border border-red-200 bg-red-50 p-4 dark:border-red-800 dark:bg-red-900/20">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
            </svg>
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading stock movements</h3>
            <div class="mt-2 text-sm text-red-700 dark:text-red-300">
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

      <!-- Data Table -->
      <div v-else-if="!loading && hasMovements" class="bg-card shadow overflow-hidden sm:rounded-md">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-border">
            <thead class="bg-muted">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-[150px]">
                  Date
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-[200px]">
                  Product
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-[120px]">
                  Type
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider w-[120px]">
                  Quantity Change
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-[200px]">
                  Location(s)
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider w-[100px]">
                  Before
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-muted-foreground uppercase tracking-wider w-[100px]">
                  After
                </th>
              </tr>
            </thead>
            <tbody class="bg-card divide-y divide-border">
              <tr v-for="movement in movements" :key="movement.id" class="hover:bg-muted/50">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-foreground">
                  {{ formatDate(movement.createdAt) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-foreground">
                  {{ getProductById(movement.productTemplateId).name }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-foreground">
                  <span :class="getMovementTypeClass(movement.movementType)">
                    {{ movement.movementType }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-right">
                  <span :class="getDeltaClass(movement.delta)">
                    {{ formatDelta(movement.delta) }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-foreground">
                  {{ formatLocations(movement) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-right text-foreground">
                  {{ calculateQuantityBefore(movement) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-right text-foreground">
                  {{ calculateQuantityAfter(movement) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="px-6 py-4 border-t border-border">
          <PaginationControl
            :pagination="pagination"
            @page-change="handlePageChange"
            @page-size-change="handlePageSizeChange"
          />
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading && !hasMovements" class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-foreground">No stock movements found</h3>
        <p class="mt-1 text-sm text-muted-foreground">
          There are no stock movements recorded for this organization yet.
        </p>
      </div>

      <!-- Loading State -->
      <div v-else-if="loading" class="flex items-center justify-center p-8">
        <div class="flex items-center space-x-2">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          <span class="text-sm text-muted-foreground">Loading stock movements...</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed, watch } from 'vue';
import { useStockMovementStore } from '@/stores/stockMovement';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import PaginationControl from './ui/PaginationControl.vue';
import { Button } from './ui/button';
import type { StockMovementDto } from '@/types/dto';

const stockMovementStore = useStockMovementStore();
const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

// Computed properties
const movements = computed(() => stockMovementStore.movements);
const loading = computed(() => stockMovementStore.loading);
const error = computed(() => stockMovementStore.error);
const pagination = computed(() => stockMovementStore.pagination);
const hasMovements = computed(() => stockMovementStore.hasMovements);

// Formatting methods
function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

function formatDelta(delta: number): string {
  return delta > 0 ? `+${delta}` : delta.toString();
}

function formatLocations(movement: StockMovementDto): string {
  const fromLocation = getLocationById(movement.fromLocationId);
  const toLocation = getLocationById(movement.toLocationId);
  
  if (movement.movementType === 'Move' && fromLocation && toLocation) {
    return `${fromLocation.name} → ${toLocation.name}`;
  } else if (fromLocation) {
    return fromLocation.name;
  } else if (toLocation) {
    return toLocation.name;
  }
  return 'Unknown Location';
}

function getMovementTypeClass(movementType: string): string {
  const baseClass = 'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium';
  
  switch (movementType) {
    case 'Add':
      return `${baseClass} bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200`;
    case 'Withdraw':
      return `${baseClass} bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200`;
    case 'Move':
      return `${baseClass} bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200`;
    case 'Reconcile':
      return `${baseClass} bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200`;
    default:
      return `${baseClass} bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200`;
  }
}

function getDeltaClass(delta: number): string {
  if (delta > 0) {
    return 'text-green-600 dark:text-green-400 font-medium';
  } else if (delta < 0) {
    return 'text-red-600 dark:text-red-400 font-medium';
  }
  return 'text-gray-600 dark:text-gray-400';
}

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
