<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import { useStockMovementStore } from '@/stores/stockMovement';
import InventoryCard from './InventoryCard.vue';
import { Button } from './ui/button';
import type { StockMovementDto } from '@/types/dto';

const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();
const stockMovementStore = useStockMovementStore();

const loading = computed(() => inventoryStore.loading || stockMovementStore.loading);
const error = computed(() => inventoryStore.error || stockMovementStore.error);
const recentMovements = computed(() => stockMovementStore.movements.slice(0, 5));
const lowStockItems = computed(() => inventoryStore.inventory.filter(item => item.quantity <= 10));

async function loadData() {
  if (!organizationStore.currentOrganizationId) {
    console.log('Waiting for organization data to be loaded...');
    return;
  }
  
  await Promise.all([
    inventoryStore.fetchInventory(1, 50),
    stockMovementStore.fetchMovements(1, 5),
    inventoryStore.fetchProducts(),
    inventoryStore.fetchLocations()
  ]);
}

async function handleRetry() {
  inventoryStore.clearError();
  stockMovementStore.clearError();
  await loadData();
}

function getProductById(id: string) {
  return inventoryStore.products.find(p => p.id === id) || { id, name: 'Unknown Product' };
}

// Load data when component mounts
onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-foreground">Dashboard</h1>
        <p class="mt-2 text-sm text-muted-foreground">
          Welcome to your warehouse management dashboard
        </p>
      </div>

      <!-- Error State -->
      <div v-if="error" class="mb-6">
        <div class="rounded-md border border-red-200 bg-red-50 p-4 dark:border-red-800 dark:bg-red-900/20">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading data</h3>
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
      </div>

      <!-- Content -->
      <div v-if="!loading && !error" class="space-y-8">
        <!-- Low Stock Alerts -->
        <div v-if="lowStockItems.length > 0" class="bg-card rounded-lg shadow p-6">
          <h2 class="text-xl font-semibold mb-4 text-foreground">Low Stock Alerts</h2>
          <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
            <div 
              v-for="item in lowStockItems" 
              :key="`${item.product.id}-${item.location.id}`"
              class="border border-yellow-200 dark:border-yellow-800 rounded-md p-3 bg-yellow-50 dark:bg-yellow-900/20"
            >
              <p class="text-sm font-medium text-yellow-800 dark:text-yellow-200">{{ item.product.name }}</p>
              <p class="text-xs text-yellow-700 dark:text-yellow-300">{{ item.location.name }}</p>
              <p class="text-sm font-bold text-yellow-900 dark:text-yellow-100 mt-1">
                Stock: {{ item.quantity }}
              </p>
            </div>
          </div>
        </div>

        <!-- Recent Movements -->
        <div class="bg-card rounded-lg shadow p-6">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-xl font-semibold text-foreground">Recent Stock Movements</h2>
            <Button variant="outline" size="sm" @click="$router.push('/movements')">
              View All
            </Button>
          </div>
          
          <div v-if="recentMovements.length === 0" class="text-center py-8">
            <p class="text-sm text-muted-foreground">No stock movements yet</p>
          </div>
          
          <div v-else class="overflow-x-auto">
            <table class="min-w-full divide-y divide-border">
              <thead class="bg-muted">
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Date</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Product</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Type</th>
                  <th class="px-4 py-3 text-right text-xs font-medium text-muted-foreground uppercase">Change</th>
                </tr>
              </thead>
              <tbody class="bg-card divide-y divide-border">
                <tr v-for="movement in recentMovements" :key="movement.id" class="hover:bg-muted/50">
                  <td class="px-4 py-3 whitespace-nowrap text-sm text-foreground">
                    {{ new Date(movement.createdAt).toLocaleDateString() }}
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-sm text-foreground">
                    {{ getProductById(movement.productTemplateId).name }}
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-sm">
                    <span class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium" 
                          :class="{
                            'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200': movement.movementType === 'Add',
                            'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200': movement.movementType === 'Withdraw',
                            'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200': movement.movementType === 'Move',
                            'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200': movement.movementType === 'Reconcile'
                          }">
                      {{ movement.movementType }}
                    </span>
                  </td>
                  <td class="px-4 py-3 whitespace-nowrap text-sm text-right" 
                      :class="movement.delta > 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'">
                    {{ movement.delta > 0 ? '+' : '' }}{{ movement.delta }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-else-if="loading" class="flex items-center justify-center p-8">
        <div class="flex items-center space-x-2">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
          <span class="text-sm text-muted-foreground">Loading dashboard...</span>
        </div>
      </div>
    </div>
  </div>
</template>

