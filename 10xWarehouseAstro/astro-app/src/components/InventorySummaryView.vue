<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold tracking-tight">Inventory Summary</h1>
        <p class="text-muted-foreground">
          View and manage inventory levels across all locations
        </p>
      </div>
      
      <!-- Action Bar -->
      <div class="flex items-center space-x-2">
        <Button
          v-if="canPerformStockOperations"
          @click="handleAddStock"
        >
          Add Stock
        </Button>
        <Button
          variant="outline"
          @click="handleRefresh"
          :disabled="loading"
        >
          Refresh
        </Button>
      </div>
    </div>

    <!-- Filter Bar -->
    <FilterBar
      :filters="filters"
      :products="products"
      :locations="locations"
      @filters-change="handleFiltersChange"
      @clear-filters="handleClearFilters"
    />

    <!-- Error State -->
    <div v-if="error" class="rounded-md border border-destructive/50 bg-destructive/10 p-4">
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-destructive" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-destructive">Error loading inventory</h3>
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
      <PaginationControl
        :pagination="pagination"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>

    <!-- Empty State -->
    <EmptyState
      v-else-if="!loading && !hasInventory"
      title="No inventory found"
      message="There are no inventory items for this organization yet."
      :action-text="canPerformStockOperations ? 'Add Stock' : undefined"
      :on-action="canPerformStockOperations ? handleAddStock : undefined"
    />

    <!-- Loading State -->
    <div v-else-if="loading" class="flex items-center justify-center p-8">
      <div class="flex items-center space-x-2">
        <div class="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
        <span class="text-sm text-muted-foreground">Loading inventory...</span>
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
</template>

<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import { Button } from './ui/button';
import PaginationControl from './ui/PaginationControl.vue';
import EmptyState from './ui/EmptyState.vue';
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
