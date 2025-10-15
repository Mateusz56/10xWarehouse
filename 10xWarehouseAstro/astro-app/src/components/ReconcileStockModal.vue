<template>
  <Dialog :open="isOpen" @update:open="onClose">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Reconcile Stock</DialogTitle>
        <DialogDescription>
          Update the actual quantity to match physical count.
        </DialogDescription>
      </DialogHeader>
      
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <!-- Product Display (Read-only) -->
        <div class="space-y-2">
          <Label>Product</Label>
          <div class="rounded-md border bg-muted px-3 py-2 text-sm">
            {{ inventory?.product.name || 'Unknown Product' }}
          </div>
        </div>

        <!-- Location Display (Read-only) -->
        <div class="space-y-2">
          <Label>Location</Label>
          <div class="rounded-md border bg-muted px-3 py-2 text-sm">
            {{ inventory?.location.name || 'Unknown Location' }}
          </div>
        </div>

        <!-- Current System Quantity Display -->
        <div class="space-y-2">
          <Label>Current System Quantity</Label>
          <div class="rounded-md border bg-muted px-3 py-2 text-sm font-medium">
            {{ inventory?.quantity || 0 }} units
          </div>
        </div>

        <!-- Physical Count Input -->
        <div class="space-y-2">
          <Label for="physicalCount">Physical Count</Label>
          <Input
            id="physicalCount"
            v-model.number="form.physicalCount"
            type="number"
            :min="0"
            required
            placeholder="Enter actual quantity found"
          />
          <p class="text-xs text-muted-foreground">
            Enter the actual quantity you counted physically
          </p>
        </div>

        <!-- Difference Display -->
        <div v-if="difference !== 0" class="rounded-md border p-3" :class="differenceClass">
          <div class="flex items-center justify-between">
            <span class="text-sm font-medium">Difference:</span>
            <span class="text-sm font-bold">
              {{ difference > 0 ? '+' : '' }}{{ difference }} units
            </span>
          </div>
          <p class="text-xs mt-1">
            {{ difference > 0 ? 'System will add' : 'System will remove' }} {{ Math.abs(difference) }} units
          </p>
        </div>

        <!-- Confirmation Checkbox -->
        <div class="flex items-center space-x-2">
          <input
            id="confirm-reconcile"
            v-model="form.confirmed"
            type="checkbox"
            class="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
          />
          <Label for="confirm-reconcile" class="text-sm">
            I confirm this physical count is accurate
          </Label>
        </div>

        <!-- Error Display -->
        <div v-if="error" class="rounded-md bg-destructive/10 p-3">
          <p class="text-sm text-destructive">{{ error }}</p>
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" @click="onClose" :disabled="loading">
            Cancel
          </Button>
          <Button type="submit" :disabled="loading || !canSubmit">
            {{ loading ? 'Reconciling...' : 'Reconcile Stock' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed, watch } from 'vue';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from './ui/dialog';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { stockMovementApi } from '@/lib/api';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import type { StockOperationModalProps, CreateStockMovementCommand } from '@/types/dto';

const props = defineProps<StockOperationModalProps>();

const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

const loading = ref(false);
const error = ref<string | null>(null);

const form = reactive<{
  physicalCount: number;
  confirmed: boolean;
}>({
  physicalCount: 0,
  confirmed: false
});

const difference = computed(() => {
  if (!props.inventory || form.physicalCount === null) return 0;
  return form.physicalCount - props.inventory.quantity;
});

const differenceClass = computed(() => {
  if (difference.value > 0) {
    return 'border-green-200 bg-green-50 text-green-800';
  } else if (difference.value < 0) {
    return 'border-red-200 bg-red-50 text-red-800';
  }
  return 'border-gray-200 bg-gray-50 text-gray-800';
});

const canSubmit = computed(() => {
  return form.physicalCount >= 0 && 
         form.physicalCount !== null &&
         form.confirmed;
});

async function handleSubmit() {
  if (!organizationStore.currentOrganizationId || !props.inventory) {
    error.value = 'Invalid operation';
    return;
  }

  loading.value = true;
  error.value = null;

  try {
    const command: CreateStockMovementCommand = {
      productTemplateId: props.inventory.product.id,
      movementType: 'Reconcile',
      delta: form.physicalCount,
      locationId: props.inventory.location.id
    };

    await stockMovementApi.createStockMovement(organizationStore.currentOrganizationId, command);
    
    // Refresh data
    await Promise.all([
      inventoryStore.fetchInventory(),
      inventoryStore.fetchMovements?.()
    ]);

    onSuccess();
    onClose();
  } catch (err) {
    console.error('Failed to reconcile stock:', err);
    error.value = err instanceof Error ? err.message : 'Failed to reconcile stock';
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  form.physicalCount = props.inventory?.quantity || 0;
  form.confirmed = false;
  error.value = null;
}

// Reset form when modal opens
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    resetForm();
  }
});

const onSuccess = () => {
  // This will be called by the parent component
};

const onClose = () => {
  resetForm();
  props.onClose();
};
</script>
