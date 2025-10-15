<template>
  <div class="flex items-center justify-between px-2 py-4">
    <div class="flex items-center space-x-2">
      <p class="text-sm text-muted-foreground">
        Showing {{ startItem }} to {{ endItem }} of {{ pagination.total }} results
      </p>
    </div>
    
    <div class="flex items-center space-x-6">
      <!-- Page Size Selector -->
      <div class="flex items-center space-x-2">
        <Label for="page-size" class="text-sm font-medium">Rows per page</Label>
        <Select :model-value="pagination.pageSize.toString()" @update:model-value="handlePageSizeChange">
          <SelectTrigger id="page-size" class="h-8 w-[70px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent side="top">
            <SelectItem v-for="size in pageSizeOptions" :key="size" :value="size.toString()">
              {{ size }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      <!-- Pagination Controls -->
      <div class="flex items-center space-x-2">
        <Button
          variant="outline"
          size="sm"
          :disabled="isFirstPage"
          @click="handlePreviousPage"
        >
          Previous
        </Button>
        
        <div class="flex items-center space-x-1">
            <Button
              v-for="page in visiblePages"
              :key="page"
              variant="outline"
              size="sm"
              :class="page === pagination.page ? 'bg-primary text-primary-foreground' : ''"
              @click="handlePageChange(typeof page === 'number' ? page : 1)"
            >
              {{ page }}
            </Button>
        </div>
        
        <Button
          variant="outline"
          size="sm"
          :disabled="isLastPage"
          @click="handleNextPage"
        >
          Next
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Button } from './button';
import { Label } from './label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './select';
import type { PaginationDto } from '@/types/dto';

interface Props {
  pagination: PaginationDto;
}

interface Emits {
  (e: 'pageChange', page: number): void;
  (e: 'pageSizeChange', pageSize: number): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const pageSizeOptions = [10, 20, 50, 100];

const totalPages = computed(() => Math.ceil(props.pagination.total / props.pagination.pageSize));
const isFirstPage = computed(() => props.pagination.page === 1);
const isLastPage = computed(() => props.pagination.page === totalPages.value);

const startItem = computed(() => {
  if (props.pagination.total === 0) return 0;
  return (props.pagination.page - 1) * props.pagination.pageSize + 1;
});

const endItem = computed(() => {
  const end = props.pagination.page * props.pagination.pageSize;
  return Math.min(end, props.pagination.total);
});

const visiblePages = computed(() => {
  const current = props.pagination.page;
  const total = totalPages.value;
  const delta = 2; // Number of pages to show on each side of current page
  
  const range = [];
  const rangeWithDots = [];
  
  for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
    range.push(i);
  }
  
  if (current - delta > 2) {
    rangeWithDots.push(1, '...');
  } else {
    rangeWithDots.push(1);
  }
  
  rangeWithDots.push(...range);
  
  if (current + delta < total - 1) {
    rangeWithDots.push('...', total);
  } else if (total > 1) {
    rangeWithDots.push(total);
  }
  
  return rangeWithDots.filter((page, index, array) => array.indexOf(page) === index);
});

function handlePageChange(page: number) {
  if (page >= 1 && page <= totalPages.value && page !== props.pagination.page) {
    emit('pageChange', page);
  }
}

function handlePageSizeChange(pageSize: any) {
  if (typeof pageSize === 'string') {
    const newPageSize = parseInt(pageSize, 10);
    if (newPageSize !== props.pagination.pageSize) {
      emit('pageSizeChange', newPageSize);
    }
  }
}

function handlePreviousPage() {
  if (!isFirstPage.value) {
    handlePageChange(props.pagination.page - 1);
  }
}

function handleNextPage() {
  if (!isLastPage.value) {
    handlePageChange(props.pagination.page + 1);
  }
}
</script>
