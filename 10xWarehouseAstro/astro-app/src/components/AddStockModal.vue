<template>
  <Dialog :open="isOpen" @update:open="onClose">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Add Stock</DialogTitle>
        <DialogDescription>
          Add inventory to a specific location.
        </DialogDescription>
      </DialogHeader>
      
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <!-- Product Selection -->
        <div class="space-y-2">
          <Label for="product">Product</Label>
          <Select v-model="form.productId" required>
            <SelectTrigger>
              <SelectValue placeholder="Select a product" />
            </SelectTrigger>
            <SelectContent>
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

        <!-- Location Selection -->
        <div class="space-y-2">
          <Label for="location">Location</Label>
          <Select v-model="form.locationId" required>
            <SelectTrigger>
              <SelectValue placeholder="Select a location" />
            </SelectTrigger>
            <SelectContent>
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

        <!-- Quantity Input -->
        <div class="space-y-2">
          <Label for="quantity">Quantity to Add</Label>
          <Input
            id="quantity"
            v-model.number="form.quantity"
            type="number"
            min="1"
            required
            placeholder="Enter quantity"
          />
        </div>

        <!-- Error Display -->
        <div v-if="error" class="rounded-md bg-destructive/10 p-3">
          <p class="text-sm text-destructive">{{ error }}</p>
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" @click="onClose" :disabled="loading">
            Cancel
          </Button>
          <Button type="submit" :disabled="loading">
            {{ loading ? 'Adding...' : 'Add Stock' }}
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
import type { StockOperationModalProps, CreateStockMovementCommand } from '@/types/dto';

const props = defineProps<StockOperationModalProps>();

const inventoryStore = useInventoryStore();
const organizationStore = useOrganizationStore();

const loading = ref(false);
const error = ref<string | null>(null);

const form = reactive<{
  productId: string;
  locationId: string;
  quantity: number;
}>({
  productId: '',
  locationId: '',
  quantity: 1
});

const products = computed(() => inventoryStore.products);
const locations = computed(() => inventoryStore.locations);

async function handleSubmit() {
  if (!organizationStore.currentOrganizationId) {
    error.value = 'No organization selected';
    return;
  }

  loading.value = true;
  error.value = null;

  try {
    const command: CreateStockMovementCommand = {
      productTemplateId: form.productId,
      movementType: 'Add',
      delta: form.quantity,
      locationId: form.locationId
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
    console.error('Failed to add stock:', err);
    error.value = err instanceof Error ? err.message : 'Failed to add stock';
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  form.productId = '';
  form.locationId = '';
  form.quantity = 1;
  error.value = null;
}

// Load data when modal opens
onMounted(async () => {
  if (props.isOpen) {
    await Promise.all([
      inventoryStore.fetchProducts(),
      inventoryStore.fetchLocations()
    ]);
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
