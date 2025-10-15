<template>
  <div class="flex flex-wrap items-center gap-4 rounded-lg border bg-card p-4">
    <!-- Product Filter -->
    <div class="flex items-center space-x-2">
      <Label for="product-filter" class="text-sm font-medium">Product</Label>
      <Select :model-value="filters.productId || 'all'" @update:model-value="handleProductChange">
        <SelectTrigger id="product-filter" class="w-[200px]">
          <SelectValue placeholder="All products" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All products</SelectItem>
          <SelectItem 
            v-for="product in products" 
            :key="product.id" 
            :value="product.id"
          >
            {{ product.name }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- Location Filter -->
    <div class="flex items-center space-x-2">
      <Label for="location-filter" class="text-sm font-medium">Location</Label>
      <Select :model-value="filters.locationId || 'all'" @update:model-value="handleLocationChange">
        <SelectTrigger id="location-filter" class="w-[200px]">
          <SelectValue placeholder="All locations" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All locations</SelectItem>
          <SelectItem 
            v-for="location in locations" 
            :key="location.id" 
            :value="location.id"
          >
            {{ location.name }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>

    <!-- Low Stock Toggle -->
    <div class="flex items-center space-x-2">
      <Label for="low-stock-toggle" class="text-sm font-medium">Low Stock Only</Label>
      <input
        id="low-stock-toggle"
        type="checkbox"
        :checked="filters.lowStock"
        @change="handleLowStockToggle"
        class="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
      />
    </div>

    <!-- Clear Filters Button -->
    <Button
      variant="outline"
      size="sm"
      @click="handleClearFilters"
      :disabled="!hasActiveFilters"
    >
      Clear Filters
    </Button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Button } from './ui/button';
import { Label } from './ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import type { InventoryFilters, ProductSummaryDto, LocationSummaryDto } from '@/types/dto';

interface Props {
  filters: InventoryFilters;
  products: ProductSummaryDto[];
  locations: LocationSummaryDto[];
}

interface Emits {
  (e: 'filtersChange', filters: InventoryFilters): void;
  (e: 'clearFilters'): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const hasActiveFilters = computed(() => {
  return !!(props.filters.productId || props.filters.locationId || props.filters.lowStock);
});

function handleProductChange(productId: any) {
  const newFilters = {
    ...props.filters,
    productId: productId === 'all' || productId === null ? undefined : String(productId)
  };
  emit('filtersChange', newFilters);
}

function handleLocationChange(locationId: any) {
  const newFilters = {
    ...props.filters,
    locationId: locationId === 'all' || locationId === null ? undefined : String(locationId)
  };
  emit('filtersChange', newFilters);
}

function handleLowStockToggle(event: Event) {
  const target = event.target as HTMLInputElement;
  const newFilters = {
    ...props.filters,
    lowStock: target.checked
  };
  emit('filtersChange', newFilters);
}

function handleClearFilters() {
  emit('clearFilters');
}
</script>
