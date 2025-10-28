<script setup lang="ts">
import { onMounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';
import { useOrganizationStore } from '@/stores/organization';
import WarehouseDetailsHeader from './WarehouseDetailsHeader.vue';
import WarehouseLocationsSection from './WarehouseLocationsSection.vue';
import CreateLocationModal from './CreateLocationModal.vue';
import EditLocationModal from './EditLocationModal.vue';
import EditWarehouseModal from './EditWarehouseModal.vue';
import DeleteConfirmationModal from './DeleteConfirmationModal.vue';

const route = useRoute();
const router = useRouter();
const warehouseDetailsStore = useWarehouseDetailsStore();
const organizationStore = useOrganizationStore();

// Get warehouse ID from route params
const warehouseId = computed(() => route.params.id as string);

const isLoading = computed(() => {
  return warehouseDetailsStore.loading;
});

const hasError = computed(() => {
  return !!warehouseDetailsStore.error;
});

const warehouse = computed(() => {
  return warehouseDetailsStore.warehouse;
});

const locations = computed(() => {
  return warehouseDetailsStore.locations;
});

const pagination = computed(() => {
  return warehouseDetailsStore.pagination;
});

// Watch for warehouse ID changes and fetch data
watch(() => warehouseId.value, async (newId) => {
  if (newId) {
    await loadWarehouseData(newId);
  }
}, { immediate: true });

// Watch for organization changes and reload data
watch(() => organizationStore.currentOrganizationId, async (newOrgId) => {
  if (newOrgId && warehouseId.value) {
    await loadWarehouseData(warehouseId.value);
  }
});

async function loadWarehouseData(id: string | null) {
  if (!id) return;
  try {
    // Clear any previous errors
    warehouseDetailsStore.clearError();
    
    // Fetch warehouse details (includes locations)
    await warehouseDetailsStore.fetchWarehouseDetails(id);
    
    // If warehouse has many locations, fetch paginated locations
    if (warehouseDetailsStore.warehouse && warehouseDetailsStore.warehouse.locations.length > 10) {
      await warehouseDetailsStore.fetchLocations(id, 1, 10);
    }
  } catch (error) {
    console.error('Failed to load warehouse data:', error);
  }
}

onMounted(async () => {
  if (warehouseId.value) {
    await loadWarehouseData(warehouseId.value);
  }
});

// Handle warehouse not found
const isWarehouseNotFound = computed(() => {
  return hasError.value && warehouseDetailsStore.error?.includes('not found');
});

// Handle unauthorized access
const isUnauthorized = computed(() => {
  return hasError.value && (
    warehouseDetailsStore.error?.includes('access') ||
    warehouseDetailsStore.error?.includes('unauthorized') ||
    warehouseDetailsStore.error?.includes('forbidden')
  );
});

// Navigation function
function navigateToWarehouses() {
  router.push('/warehouses');
}
</script>

<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="isLoading && !warehouse" class="flex items-center justify-center py-12">
        <div class="flex items-center space-x-2 text-muted-foreground">
          <svg class="animate-spin w-6 h-6" fill="none" viewBox="0 0 24 24" aria-hidden="true">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span class="text-lg">Loading warehouse details...</span>
        </div>
      </div>

      <!-- Error States -->
      <div v-else-if="hasError" class="max-w-2xl mx-auto">
        <!-- Warehouse Not Found -->
        <div v-if="isWarehouseNotFound" class="text-center py-12">
          <div class="flex flex-col items-center">
            <div class="flex items-center justify-center w-16 h-16 bg-muted rounded-full mb-4">
              <svg class="w-8 h-8 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
              </svg>
            </div>
            <h1 class="text-2xl font-bold text-foreground mb-2">
              Warehouse Not Found
            </h1>
            <p class="text-muted-foreground mb-6 max-w-md">
              The warehouse you're looking for doesn't exist or you don't have access to it.
            </p>
            <button
              @click="navigateToWarehouses"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-primary-foreground bg-primary hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
              Back to Warehouses
            </button>
          </div>
        </div>

        <!-- Unauthorized Access -->
        <div v-else-if="isUnauthorized" class="text-center py-12">
          <div class="flex flex-col items-center">
            <div class="flex items-center justify-center w-16 h-16 bg-destructive/10 rounded-full mb-4">
              <svg class="w-8 h-8 text-destructive" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.268 19.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <h1 class="text-2xl font-bold text-foreground mb-2">
              Access Denied
            </h1>
            <p class="text-muted-foreground mb-6 max-w-md">
              You don't have permission to view this warehouse. Please contact your organization administrator.
            </p>
            <button
              @click="navigateToWarehouses"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-primary-foreground bg-primary hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-ring"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
              Back to Warehouses
            </button>
          </div>
        </div>

        <!-- Generic Error -->
        <div v-else class="bg-destructive/10 border border-destructive/50 rounded-lg p-6">
          <div class="flex items-center">
            <svg class="w-5 h-5 text-destructive mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h3 class="text-sm font-medium text-destructive">
                Error loading warehouse
              </h3>
              <p class="text-sm text-destructive mt-1">
                {{ warehouseDetailsStore.error }}
              </p>
            </div>
          </div>
          <div class="mt-4">
            <button
              @click="() => loadWarehouseData(warehouseId)"
              class="inline-flex items-center px-3 py-2 border border-transparent text-sm font-medium rounded-md text-destructive-foreground bg-destructive hover:bg-destructive/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-destructive"
            >
              Try Again
            </button>
          </div>
        </div>
      </div>

      <!-- Main Content -->
      <div v-else-if="warehouse" class="space-y-8">
        <!-- Warehouse Header -->
        <WarehouseDetailsHeader :warehouse="warehouse" />

        <!-- Locations Section -->
        <WarehouseLocationsSection
          :locations="locations"
          :pagination="pagination"
          :loading="isLoading"
        />
      </div>

      <!-- Modals -->
      <CreateLocationModal />
      <EditLocationModal />
      <EditWarehouseModal />
      <DeleteConfirmationModal />
    </div>
  </div>
</template>
