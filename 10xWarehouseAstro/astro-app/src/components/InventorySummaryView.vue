<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Inventory Summary</h1>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              View and manage inventory levels across all locations
            </p>
          </div>
          
          <!-- Action Bar -->
          <div class="flex items-center space-x-2">
            <Button
              v-if="canPerformStockOperations"
              @click="handleAddStock"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Add Stock
            </Button>
            <Button
              variant="outline"
              @click="handleRefresh"
              :disabled="loading"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Refresh
            </Button>
          </div>
        </div>
      </div>

      <!-- Filter Bar -->
      <div class="mb-6">
        <FilterBar
          :filters="filters"
          :products="products"
          :locations="locations"
          @filters-change="handleFiltersChange"
          @clear-filters="handleClearFilters"
        />
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
            <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading inventory</h3>
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

      <!-- Inventory Grid -->
      <div v-else-if="!loading && hasInventory">
        <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          <InventoryCard
            v-for="item in inventory"
            :key="`${item.product.id}-${item.location.id}`"
            :inventory="item"
            :user-role="userRole"
            @move="handleMove"
            @withdraw="handleWithdraw"
            @reconcile="handleReconcile"
          />
        </div>

        <!-- Pagination -->
        <div class="mt-6">
          <PaginationControl
            :pagination="pagination"
            @page-change="handlePageChange"
            @page-size-change="handlePageSizeChange"
          />
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading && !hasInventory" class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-gray-100">No inventory found</h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          There are no inventory items for this organization yet.
        </p>
        <div v-if="canPerformStockOperations" class="mt-6">
          <Button @click="handleAddStock">
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Add Stock
          </Button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-else-if="loading" class="flex items-center justify-center p-8">
        <div class="flex items-center space-x-2">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-blue-600 border-t-transparent"></div>
          <span class="text-sm text-gray-600 dark:text-gray-400">Loading inventory...</span>
        </div>
      </div>

      <!-- Stock Operation Modals -->
      <AddStockModal
        :is-open="showAddStockModal"
        :on-close="closeModals"
        :on-success="handleStockOperationSuccess"
      />

      <MoveStockModal
        :is-open="showMoveStockModal"
        :inventory="selectedInventory"
        :on-close="closeModals"
        :on-success="handleStockOperationSuccess"
      />

      <WithdrawStockModal
        :is-open="showWithdrawStockModal"
        :inventory="selectedInventory"
        :on-close="closeModals"
        :on-success="handleStockOperationSuccess"
      />

      <ReconcileStockModal
        :is-open="showReconcileStockModal"
        :inventory="selectedInventory"
        :on-close="closeModals"
        :on-success="handleStockOperationSuccess"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import PaginationControl from './ui/PaginationControl.vue';
import { Button } from './ui/button';
import FilterBar from './FilterBar.vue';
import InventoryCard from './InventoryCard.vue';
import AddStockModal from './AddStockModal.vue';
import MoveStockModal from './MoveStockModal.vue';
import WithdrawStockModal from './WithdrawStockModal.vue';
import ReconcileStockModal from './ReconcileStockModal.vue';
import type { UserRole, InventorySummaryDto } from '@/types/dto';

const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

// Computed properties
const inventory = computed(() => inventoryStore.inventory);
const products = computed(() => inventoryStore.products);
const locations = computed(() => inventoryStore.locations);
const loading = computed(() => inventoryStore.loading);
const error = computed(() => inventoryStore.error);
const pagination = computed(() => inventoryStore.pagination);
const filters = computed(() => inventoryStore.filters);
const hasInventory = computed(() => inventoryStore.hasInventory);
const canPerformStockOperations = computed(() => inventoryStore.canPerformStockOperations);

const userRole = computed(() => organizationStore.currentRole as UserRole);

// Modal state
const showAddStockModal = ref(false);
const showMoveStockModal = ref(false);
const showWithdrawStockModal = ref(false);
const showReconcileStockModal = ref(false);
const selectedInventory = ref<InventorySummaryDto | undefined>(undefined);

// Methods
async function loadData() {
  // Wait for organization data to be loaded
  if (!organizationStore.currentOrganizationId) {
    console.log('Waiting for organization data to be loaded...');
    return;
  }
  
  await Promise.all([
    inventoryStore.fetchInventory(),
    inventoryStore.fetchProducts(),
    inventoryStore.fetchLocations()
  ]);
}

function handlePageChange(page: number) {
  inventoryStore.setPage(page);
  inventoryStore.fetchInventory();
}

function handlePageSizeChange(pageSize: number) {
  inventoryStore.setPageSize(pageSize);
  inventoryStore.fetchInventory();
}

function handleFiltersChange(newFilters: typeof filters.value) {
  inventoryStore.setFilters(newFilters);
  inventoryStore.fetchInventory();
}

function handleClearFilters() {
  inventoryStore.clearFilters();
  inventoryStore.fetchInventory();
}

function handleRefresh() {
  loadData();
}

function handleRetry() {
  inventoryStore.clearError();
  loadData();
}

function handleAddStock() {
  showAddStockModal.value = true;
}

function handleMove(inventory: InventorySummaryDto) {
  selectedInventory.value = inventory;
  showMoveStockModal.value = true;
}

function handleWithdraw(inventory: InventorySummaryDto) {
  selectedInventory.value = inventory;
  showWithdrawStockModal.value = true;
}

function handleReconcile(inventory: InventorySummaryDto) {
  selectedInventory.value = inventory;
  showReconcileStockModal.value = true;
}

function closeModals() {
  showAddStockModal.value = false;
  showMoveStockModal.value = false;
  showWithdrawStockModal.value = false;
  showReconcileStockModal.value = false;
  selectedInventory.value = undefined;
}

async function handleStockOperationSuccess() {
  // Refresh data after successful stock operation
  await loadData();
}

// Watchers
watch(
  () => organizationStore.currentOrganizationId,
  (newOrgId) => {
    if (newOrgId) {
      console.log('Organization loaded, fetching inventory data...');
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
