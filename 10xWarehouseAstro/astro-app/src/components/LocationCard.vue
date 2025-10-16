<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';
import type { LocationVM } from '@/types/dto';
import { Button } from '@/components/ui/button';

const props = defineProps<{
  location: LocationVM;
}>();

const uiStore = useUiStore();
const warehouseDetailsStore = useWarehouseDetailsStore();

function handleEditClick(event: Event) {
  event.stopPropagation();
  uiStore.openEditLocationModal(props.location);
}

function handleDeleteClick(event: Event) {
  event.stopPropagation();
  uiStore.openDeleteLocationModal(props.location);
}
</script>

<template>
  <div class="bg-card rounded-lg border border-border p-6 hover:shadow-md transition-shadow">
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <h3 class="text-lg font-semibold text-foreground mb-2">
          {{ location.name }}
        </h3>
        <div v-if="location.description" class="text-sm text-muted-foreground mb-3">
          {{ location.description }}
        </div>
        <div class="flex items-center text-sm text-muted-foreground">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
          </svg>
          <span>Location ID: {{ location.id.slice(0, 8) }}...</span>
        </div>
      </div>
      
      <div 
        v-if="warehouseDetailsStore.canEditLocation || warehouseDetailsStore.canDeleteLocation"
        class="flex items-center space-x-2 ml-4"
      >
        <Button
          v-if="warehouseDetailsStore.canEditLocation"
          variant="outline"
          size="sm"
          @click="handleEditClick"
          :aria-label="`Edit location ${location.name}`"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
        </Button>
        
        <Button
          v-if="warehouseDetailsStore.canDeleteLocation"
          variant="outline"
          size="sm"
          @click="handleDeleteClick"
          :aria-label="`Delete location ${location.name}`"
          class="text-red-600 hover:text-red-700 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/20"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </Button>
      </div>
    </div>
  </div>
</template>
