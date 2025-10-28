<script setup lang="ts">
import { useUiStore } from '@/stores/ui';
import { useWarehouseStore } from '@/stores/warehouse';
import { useRouter } from 'vue-router';
import type { WarehouseVM } from '@/types/dto';
import { Button } from '@/components/ui/button';

const props = defineProps<{
  warehouse: WarehouseVM;
}>();

const router = useRouter();
const uiStore = useUiStore();
const warehouseStore = useWarehouseStore();

function handleCardClick() {
  router.push(`/warehouses/${props.warehouse.id}`);
}

function handleEditClick(event: Event) {
  event.stopPropagation();
  uiStore.openEditWarehouseModal(props.warehouse);
}

function handleDeleteClick(event: Event) {
  event.stopPropagation();
  uiStore.openDeleteWarehouseModal(props.warehouse);
}
</script>

<template>
  <div 
    class="bg-card rounded-lg border border-border p-6 hover:shadow-md transition-shadow cursor-pointer focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
    @click="handleCardClick"
    @keydown.enter="handleCardClick"
    @keydown.space.prevent="handleCardClick"
    tabindex="0"
    role="button"
    :aria-label="`View warehouse ${warehouse.name}`"
  >
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <h3 class="text-lg font-semibold text-foreground">
          {{ warehouse.name }}
        </h3>
      </div>
      
      <div 
        v-if="warehouseStore.canEditWarehouse || warehouseStore.canDeleteWarehouse"
        class="flex items-center space-x-2 ml-4"
        @click.stop
      >
        <Button
          v-if="warehouseStore.canEditWarehouse"
          variant="outline"
          size="sm"
          @click="handleEditClick"
          :aria-label="`Edit warehouse ${warehouse.name}`"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
        </Button>
        
        <Button
          v-if="warehouseStore.canDeleteWarehouse"
          variant="outline"
          size="sm"
          @click="handleDeleteClick"
          :aria-label="`Delete warehouse ${warehouse.name}`"
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
