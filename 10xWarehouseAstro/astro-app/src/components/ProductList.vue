<script setup lang="ts">
import { computed } from 'vue';
import { useProductStore } from '@/stores/product';
import { useUiStore } from '@/stores/ui';
import ProductCard from './ProductCard.vue';
import PaginationControl from './PaginationControl.vue';
import type { ProductTemplateVM, PaginationDto } from '@/types/dto';

const props = defineProps<{
  products: ProductTemplateVM[];
  loading: boolean;
  pagination: PaginationDto;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}>();

const productStore = useProductStore();
const uiStore = useUiStore();

const isEmpty = computed(() => !props.loading && props.products.length === 0);

function handlePageChange(page: number) {
  productStore.setPage(page);
  productStore.fetchProducts(page, props.pagination.pageSize, productStore.searchQuery);
}

function handlePageSizeChange(size: number) {
  productStore.setPageSize(size);
  productStore.fetchProducts(1, size, productStore.searchQuery);
}
</script>

<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="n in 6" 
        :key="n"
        class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6 animate-pulse"
      >
        <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded mb-4"></div>
        <div class="space-y-2">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="isEmpty" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No products</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
        Get started by creating your first product template.
      </p>
      <div v-if="canCreate" class="mt-6">
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

    <!-- Product Cards -->
    <div v-else class="space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6" role="grid" aria-label="Product list">
        <div
          v-for="product in products"
          :key="product.id"
          role="gridcell"
        >
          <ProductCard 
            :product="product" 
            :can-edit="canEdit"
            :can-delete="canDelete"
          />
        </div>
      </div>

      <!-- Pagination -->
      <PaginationControl
        :pagination="pagination"
        @page-change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>
  </div>
</template>
