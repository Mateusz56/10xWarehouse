<template>
  <Dialog :open="isOpen" @update:open="onClose">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Move Stock</DialogTitle>
        <DialogDescription>
          Move inventory from one location to another.
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

        <!-- From Location Display (Read-only) -->
        <div class="space-y-2">
          <Label>From Location</Label>
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

        <!-- To Location Selection -->
        <div class="space-y-2">
          <Label for="toLocation">To Location</Label>
          <Select v-model="form.toLocationId" required>
            <SelectTrigger>
              <SelectValue placeholder="Select destination location" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem 
                v-for="location in availableLocations" 
                :key="location.id" 
                :value="location.id"
              >
                {{ location.name }}
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <!-- Quantity Input -->
        <div class="space-y-2">
          <Label for="quantity">Quantity to Move</Label>
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

        <!-- Error Display -->
        <div v-if="error" class="rounded-md bg-destructive/10 p-3">
          <p class="text-sm text-destructive">{{ error }}</p>
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" @click="onClose" :disabled="loading">
            Cancel
          </Button>
          <Button type="submit" :disabled="loading || !canSubmit">
            {{ loading ? 'Moving...' : 'Move Stock' }}
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { stockMovementApi } from '@/lib/api';
import { useInventoryStore } from '@/stores/inventory';
import { useOrganizationStore } from '@/stores/organization';
import type { StockOperationModalProps, CreateStockMovementCommand, LocationSummaryDto } from '@/types/dto';

const props = defineProps<StockOperationModalProps>();

const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

const loading = ref(false);
const error = ref<string | null>(null);

const form = reactive<{
  toLocationId: string;
  quantity: number;
}>({
  toLocationId: '',
  quantity: 1
});

const locations = computed(() => inventoryStore.locations);

// Filter out the current location from available destinations
const availableLocations = computed(() => {
  if (!props.inventory) return locations.value;
  return locations.value.filter(location => location.id !== props.inventory?.location.id);
});

const canSubmit = computed(() => {
  return form.toLocationId && 
         form.quantity > 0 && 
         form.quantity <= (props.inventory?.quantity || 0) &&
         form.toLocationId !== props.inventory?.location.id;
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
      movementType: 'Move',
      delta: form.quantity,
      fromLocationId: props.inventory.location.id,
      toLocationId: form.toLocationId
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
    console.error('Failed to move stock:', err);
    error.value = err instanceof Error ? err.message : 'Failed to move stock';
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  form.toLocationId = '';
  form.quantity = 1;
  error.value = null;
}

// Load data when modal opens
onMounted(async () => {
  if (props.isOpen) {
    await inventoryStore.fetchLocations();
    resetForm();
  }
});

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
