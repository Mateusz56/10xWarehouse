<script setup lang="ts">
import { onMounted, watch, onErrorCaptured } from 'vue';
import { useWarehouseStore } from '@/stores/warehouse';
import { useOrganizationStore } from '@/stores/organization';
import WarehouseHeader from './WarehouseHeader.vue';
import WarehouseList from './WarehouseList.vue';
import PaginationControl from './PaginationControl.vue';
import { Button } from './ui/button';
import CreateWarehouseModal from './CreateWarehouseModal.vue';
import EditWarehouseModal from './EditWarehouseModal.vue';
import DeleteConfirmationModal from './DeleteConfirmationModal.vue';

const warehouseStore = useWarehouseStore();
const organizationStore = useOrganizationStore();

// Global error handler for this component tree
onErrorCaptured((error, instance, info) => {
  console.error('WarehousePageView Error:', error, info);
  warehouseStore.error = error.message || 'An unexpected error occurred';
  return false; // Prevent the error from propagating further
});

// Fetch warehouses when component mounts or organization changes
onMounted(async () => {
  try {
    if (organizationStore.currentOrganizationId) {
      await warehouseStore.fetchWarehouses();
    }
  } catch (error) {
    console.error('Failed to fetch warehouses on mount:', error);
  }
});

// Watch for organization changes
watch(() => organizationStore.currentOrganizationId, async (newOrgId) => {
  try {
    if (newOrgId) {
      warehouseStore.clearWarehouses();
      await warehouseStore.fetchWarehouses();
    } else {
      warehouseStore.clearWarehouses();
    }
  } catch (error) {
    console.error('Failed to fetch warehouses on organization change:', error);
  }
});

// Handle pagination changes
async function handlePageChange(page: number) {
  try {
    warehouseStore.setPage(page);
    await warehouseStore.fetchWarehouses();
  } catch (error) {
    console.error('Failed to change page:', error);
  }
}

async function handlePageSizeChange(pageSize: number) {
  try {
    warehouseStore.setPageSize(pageSize);
    await warehouseStore.fetchWarehouses();
  } catch (error) {
    console.error('Failed to change page size:', error);
  }
}

async function handleRetry() {
  try {
    warehouseStore.clearError();
    await warehouseStore.fetchWarehouses();
  } catch (error) {
    console.error('Failed to retry operation:', error);
  }
}
</script>

<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <WarehouseHeader />
      
      <!-- Error State -->
      <div v-if="warehouseStore.error" class="mb-6">
        <div class="bg-destructive/10 border border-destructive/50 rounded-md p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-destructive" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-destructive">
                Error loading warehouses
              </h3>
              <div class="mt-2 text-sm text-destructive">
                <p>{{ warehouseStore.error }}</p>
              </div>
              <div class="mt-4">
                <Button variant="outline" size="sm" @click="handleRetry">
                  Try again
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Main Content -->
      <div class="bg-card rounded-lg shadow">
        <!-- Warehouse List -->
        <div class="p-6">
          <WarehouseList 
            :warehouses="warehouseStore.warehouses"
            :loading="warehouseStore.loading"
          />
        </div>
        
        <!-- Pagination -->
        <div v-if="warehouseStore.hasWarehouses && !warehouseStore.loading" class="px-6 py-4 border-t border-border">
          <PaginationControl
            :pagination="warehouseStore.pagination"
            @page-change="handlePageChange"
            @page-size-change="handlePageSizeChange"
          />
        </div>
      </div>
    </div>
    
    <!-- Modals -->
    <CreateWarehouseModal />
    <EditWarehouseModal />
    <DeleteConfirmationModal />
  </div>
</template>
