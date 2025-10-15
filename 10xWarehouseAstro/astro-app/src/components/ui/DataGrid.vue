<template>
  <div class="w-full">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center p-8">
      <div class="flex items-center space-x-2">
        <div class="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent"></div>
        <span class="text-sm text-muted-foreground">Loading...</span>
      </div>
    </div>

    <!-- Data Grid -->
    <div v-else-if="data.length > 0" class="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead 
              v-for="column in columns" 
              :key="column.key"
              :class="column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'"
              :style="{ width: column.width }"
            >
              {{ column.label }}
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <slot name="rows" :data="data" />
        </TableBody>
      </Table>
    </div>

    <!-- Empty State -->
    <div v-else class="flex flex-col items-center justify-center p-8 text-center">
      <div class="text-muted-foreground">
        <slot name="empty">
          <p>No data available</p>
        </slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Table, TableHeader, TableBody, TableRow, TableHead } from './table';
import type { DataGridColumn } from '@/types/dto';

interface Props<T = any> {
  data: T[];
  columns: DataGridColumn[];
  loading?: boolean;
}

defineProps<Props>();
</script>
