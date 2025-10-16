<script setup lang="ts">
import type { LocationVM } from '@/types/dto';
import LocationCard from './LocationCard.vue';

const props = defineProps<{
  locations: LocationVM[];
  loading: boolean;
}>();

const isEmpty = props.locations.length === 0 && !props.loading;
</script>

<template>
  <div>
    <!-- Empty State -->
    <div v-if="isEmpty" class="text-center py-12">
      <div class="flex flex-col items-center">
        <div class="flex items-center justify-center w-16 h-16 bg-muted rounded-full mb-4">
          <svg class="w-8 h-8 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
          </svg>
        </div>
        <h3 class="text-lg font-medium text-foreground mb-2">
          No locations yet
        </h3>
        <p class="text-muted-foreground mb-6 max-w-sm">
          Get started by creating your first storage location in this warehouse.
        </p>
        <div class="text-sm text-muted-foreground">
          <p>Locations help organize inventory within your warehouse.</p>
          <p class="mt-1">Examples: "Aisle 1", "Cold Storage", "Loading Dock"</p>
        </div>
      </div>
    </div>

    <!-- Locations Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <LocationCard
        v-for="location in locations"
        :key="location.id"
        :location="location"
      />
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="n in 6"
        :key="n"
        class="bg-card rounded-lg border border-border p-6 animate-pulse"
      >
        <div class="flex items-start justify-between">
          <div class="flex-1">
            <div class="h-6 bg-muted rounded mb-2"></div>
            <div class="h-4 bg-muted rounded mb-3 w-3/4"></div>
            <div class="h-4 bg-muted rounded w-1/2"></div>
          </div>
          <div class="flex items-center space-x-2 ml-4">
            <div class="w-8 h-8 bg-muted rounded"></div>
            <div class="w-8 h-8 bg-muted rounded"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
