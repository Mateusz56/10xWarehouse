<template>
  <Dialog :open="isOpen" @update:open="onClose">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Withdraw Stock</DialogTitle>
        <DialogDescription>
          Remove inventory from a location.
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

        <!-- Current Quantity Display -->
        <div class="space-y-2">
          <Label>Available Quantity</Label>
          <div class="rounded-md border bg-muted px-3 py-2 text-sm font-medium">
            {{ inventory?.quantity || 0 }} units
          </div>
        </div>

        <!-- Quantity Input -->
        <div class="space-y-2">
          <Label for="quantity">Quantity to Withdraw</Label>
          <Input
            id="quantity"
            v-model.number="form.quantity"
            type="number"
            :min="1"
            :max="inventory?.quantity || 0"
            required
            placeholder="Enter quantity"
          />
          <p class="text-xs text-muted-foreground">
            Maximum: {{ inventory?.quantity || 0 }} units
          </p>
        </div>

        <!-- Confirmation Checkbox -->
        <div class="flex items-center space-x-2">
          <input
            id="confirm-withdraw"
            v-model="form.confirmed"
            type="checkbox"
            class="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
          />
          <Label for="confirm-withdraw" class="text-sm">
            I confirm that I want to withdraw this stock
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
          <Button type="submit" :disabled="loading || !canSubmit" variant="destructive">
            {{ loading ? 'Withdrawing...' : 'Withdraw Stock' }}
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
  quantity: number;
  confirmed: boolean;
}>({
  quantity: 1,
  confirmed: false
});

const canSubmit = computed(() => {
  return form.quantity > 0 && 
         form.quantity <= (props.inventory?.quantity || 0) &&
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
      movementType: 'Withdraw',
      delta: form.quantity,
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
    console.error('Failed to withdraw stock:', err);
    error.value = err instanceof Error ? err.message : 'Failed to withdraw stock';
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  form.quantity = 1;
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
