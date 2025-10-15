<script setup lang="ts">
import { onMounted, watch, onErrorCaptured } from 'vue';
import { useProductStore } from '@/stores/product';
import { useOrganizationStore } from '@/stores/organization';
import { useUiStore } from '@/stores/ui';
import ProductList from './ProductList.vue';
import CreateProductModal from './CreateProductModal.vue';
import EditProductModal from './EditProductModal.vue';
import DeleteProductModal from './DeleteProductModal.vue';

const productStore = useProductStore();
const organizationStore = useOrganizationStore();
const uiStore = useUiStore();

// Global error handler for this component tree
onErrorCaptured((error, instance, info) => {
  console.error('ProductPageView Error:', error, info);
  productStore.error = error.message || 'An unexpected error occurred';
  return false; // Prevent the error from propagating further
});

// Fetch products when component mounts or organization changes
onMounted(async () => {
  try {
    if (organizationStore.currentOrganizationId) {
      await productStore.fetchProducts();
    }
  } catch (error) {
    console.error('Failed to fetch products on mount:', error);
  }
});

// Watch for organization changes
watch(() => organizationStore.currentOrganizationId, async (newOrgId) => {
  try {
    if (newOrgId) {
      productStore.clearProducts();
      await productStore.fetchProducts();
    } else {
      productStore.clearProducts();
    }
  } catch (error) {
    console.error('Failed to fetch products on organization change:', error);
  }
});

async function handleRetry() {
  try {
    productStore.clearError();
    await productStore.fetchProducts();
  } catch (error) {
    console.error('Failed to retry operation:', error);
  }
}

// Debounced search handler
let searchTimeout: NodeJS.Timeout | null = null;
async function handleSearch() {
  if (searchTimeout) {
    clearTimeout(searchTimeout);
  }
  
  searchTimeout = setTimeout(async () => {
    try {
      productStore.setSearchQuery(productStore.searchQuery);
      await productStore.fetchProducts(1, productStore.pageSize, productStore.searchQuery);
    } catch (error) {
      console.error('Failed to search products:', error);
    }
  }, 300); // 300ms debounce
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Products</h1>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Manage your product templates and inventory items
            </p>
          </div>
          
          <div v-if="productStore.canCreateProduct" class="flex items-center space-x-4">
            <button
              @click="uiStore.openCreateProductModal()"
              class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Create Product
            </button>
          </div>
        </div>
        
        <!-- Search Bar -->
        <div class="mt-6">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <input
              type="text"
              v-model="productStore.searchQuery"
              @input="handleSearch"
              placeholder="Search products by name or barcode..."
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md leading-5 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            />
          </div>
        </div>
      </div>
      
      <!-- Error State -->
      <div v-if="productStore.error" class="mb-6">
        <div class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                Error loading products
              </h3>
              <div class="mt-2 text-sm text-red-700 dark:text-red-300">
                <p>{{ productStore.error }}</p>
              </div>
              <div class="mt-4">
                <button
                  @click="handleRetry"
                  class="bg-red-50 dark:bg-red-900/20 px-2 py-1.5 rounded-md text-sm font-medium text-red-800 dark:text-red-200 hover:bg-red-100 dark:hover:bg-red-900/30"
                >
                  Try again
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Main Content -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow">
        <!-- Product List -->
        <div class="p-6">
          <ProductList 
            :products="productStore.products"
            :loading="productStore.loading"
            :pagination="productStore.pagination"
            :can-create="productStore.canCreateProduct"
            :can-edit="productStore.canEditProduct"
            :can-delete="productStore.canDeleteProduct"
          />
        </div>
      </div>
    </div>
    
    <!-- Modals -->
    <CreateProductModal />
    <EditProductModal />
    <DeleteProductModal />
  </div>
</template>
