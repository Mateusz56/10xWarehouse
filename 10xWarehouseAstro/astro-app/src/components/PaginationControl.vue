<script setup lang="ts">
import { computed } from 'vue';
import { Button } from '@/components/ui/button';
import type { PaginationDto } from '@/types/dto';

const props = defineProps<{
  pagination: PaginationDto;
}>();

const emit = defineEmits<{
  pageChange: [page: number];
  pageSizeChange: [pageSize: number];
}>();

const pageNumbers = computed(() => {
  const pages = [];
  const current = props.pagination?.page || 1;
  const totalPages = Math.ceil((props.pagination?.total || 0) / (props.pagination?.pageSize || 10));
  
  // Show up to 5 page numbers
  let start = Math.max(1, current - 2);
  let end = Math.min(totalPages, start + 4);
  
  // Adjust start if we're near the end
  if (end - start < 4) {
    start = Math.max(1, end - 4);
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i);
  }
  
  return pages;
});

const pageSizeOptions = [10, 25, 50, 100];

function handlePageChange(page: number) {
  emit('pageChange', page);
}

function handlePageSizeChange(event: Event) {
  const target = event.target as HTMLSelectElement;
  emit('pageSizeChange', parseInt(target.value));
}
</script>

<template>
  <div class="flex items-center justify-between px-4 py-3 bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
    <div class="flex items-center text-sm text-gray-700 dark:text-gray-300">
      <span>
        Showing {{ ((pagination?.page || 1) - 1) * (pagination?.pageSize || 10) + 1 }} to 
        {{ Math.min((pagination?.page || 1) * (pagination?.pageSize || 10), pagination?.total || 0) }} of 
        {{ pagination?.total || 0 }} results
      </span>
    </div>
    
    <div class="flex items-center space-x-4">
      <!-- Page Size Selector -->
      <div class="flex items-center space-x-2">
        <label for="pageSize" class="text-sm text-gray-700 dark:text-gray-300">Show:</label>
        <select
          id="pageSize"
          :value="pagination?.pageSize || 10"
          @change="handlePageSizeChange"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md px-2 py-1 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        >
          <option v-for="size in pageSizeOptions" :key="size" :value="size">
            {{ size }}
          </option>
        </select>
      </div>
      
      <!-- Pagination Controls -->
      <div class="flex items-center space-x-1">
        <!-- Previous Button -->
        <Button
          variant="outline"
          size="sm"
          :disabled="(pagination?.page || 1) === 1"
          @click="handlePageChange((pagination?.page || 1) - 1)"
          aria-label="Go to previous page"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </Button>
        
        <!-- Page Numbers -->
        <template v-for="page in pageNumbers" :key="page">
          <Button
            v-if="page === (pagination?.page || 1)"
            variant="default"
            size="sm"
            class="bg-blue-600 text-white hover:bg-blue-700"
            :aria-label="`Current page ${page}`"
            aria-current="page"
          >
            {{ page }}
          </Button>
          <Button
            v-else
            variant="outline"
            size="sm"
            @click="handlePageChange(page)"
            :aria-label="`Go to page ${page}`"
          >
            {{ page }}
          </Button>
        </template>
        
        <!-- Next Button -->
        <Button
          variant="outline"
          size="sm"
          :disabled="(pagination?.page || 1) === Math.ceil((pagination?.total || 0) / (pagination?.pageSize || 10))"
          @click="handlePageChange((pagination?.page || 1) + 1)"
          aria-label="Go to next page"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </Button>
      </div>
    </div>
  </div>
</template>
