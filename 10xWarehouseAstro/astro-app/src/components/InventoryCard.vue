<template>
  <div class="rounded-lg border bg-card p-6 shadow-sm transition-shadow hover:shadow-md">
    <div class="flex items-start justify-between">
      <div class="space-y-2">
        <h3 class="text-lg font-semibold">{{ inventory.product.name }}</h3>
        <p class="text-sm text-muted-foreground">{{ inventory.location.name }}</p>
        <div class="flex items-center space-x-2">
          <span class="text-2xl font-bold" :class="quantityClass">
            {{ inventory.quantity }}
          </span>
          <span v-if="isLowStock" class="rounded-full bg-yellow-100 px-2 py-1 text-xs font-medium text-yellow-800">
            Low Stock
          </span>
        </div>
      </div>
      
      <div v-if="canPerformOperations" class="flex space-x-2">
        <Button
          variant="outline"
          size="sm"
          @click="handleMove"
          :disabled="inventory.quantity === 0"
        >
          Move
        </Button>
        <Button
          variant="outline"
          size="sm"
          @click="handleWithdraw"
          :disabled="inventory.quantity === 0"
        >
          Withdraw
        </Button>
        <Button
          variant="outline"
          size="sm"
          @click="handleReconcile"
        >
          Reconcile
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Button } from './ui/button';
import type { InventoryCardProps } from '@/types/dto';

const props = defineProps<InventoryCardProps>();

const emit = defineEmits<{
  move: [inventory: typeof props.inventory];
  withdraw: [inventory: typeof props.inventory];
  reconcile: [inventory: typeof props.inventory];
}>();

const isLowStock = computed(() => {
  // Use the isLowStock flag from the backend
  return props.inventory.isLowStock;
});

const quantityClass = computed(() => {
  if (props.inventory.quantity === 0) {
    return 'text-red-600';
  } else if (isLowStock.value) {
    return 'text-yellow-600';
  } else {
    return 'text-green-600';
  }
});

const canPerformOperations = computed(() => {
  return props.userRole === 'Owner' || props.userRole === 'Member';
});

function handleMove() {
  emit('move', props.inventory);
}

function handleWithdraw() {
  emit('withdraw', props.inventory);
}

function handleReconcile() {
  emit('reconcile', props.inventory);
}
</script>
