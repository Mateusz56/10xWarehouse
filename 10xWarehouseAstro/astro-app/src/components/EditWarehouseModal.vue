<script setup lang="ts">
import { ref, watch } from 'vue';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import * as z from 'zod';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { useUiStore } from '@/stores/ui';
import { useWarehouseStore } from '@/stores/warehouse';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';

const uiStore = useUiStore();
const warehouseStore = useWarehouseStore();
const warehouseDetailsStore = useWarehouseDetailsStore();
const error = ref<string | null>(null);

const formSchema = toTypedSchema(z.object({
  name: z.string()
    .min(1, 'Name is required.')
    .max(100, 'Name must be at most 100 characters.')
    .regex(/^[a-zA-Z0-9\s\-_]+$/, 'Name can only contain letters, numbers, spaces, hyphens, and underscores.'),
}));

const { handleSubmit, isSubmitting, resetForm, setFieldValue } = useForm({
  validationSchema: formSchema,
});

// Watch for selected warehouse changes and populate form
watch(() => uiStore.selectedWarehouse, (warehouse) => {
  if (warehouse) {
    setFieldValue('name', warehouse.name);
  }
}, { immediate: true });

// Watch for selected warehouse details changes and populate form
watch(() => uiStore.selectedWarehouseDetails, (warehouse) => {
  if (warehouse) {
    setFieldValue('name', warehouse.name);
  }
}, { immediate: true });

const onSubmit = handleSubmit(async (values) => {
  const selectedWarehouse = uiStore.selectedWarehouse || uiStore.selectedWarehouseDetails;
  if (!selectedWarehouse) return;
  
  error.value = null;
  try {
    // Use appropriate store based on context
    if (uiStore.selectedWarehouseDetails) {
      await warehouseDetailsStore.updateWarehouse(selectedWarehouse.id, values);
    } else {
      await warehouseStore.updateWarehouse(selectedWarehouse.id, values);
    }
    
    // Close the appropriate modal based on context
    if (uiStore.selectedWarehouseDetails) {
      uiStore.closeEditWarehouseDetailsModal();
    } else {
      uiStore.closeEditWarehouseModal();
    }
    resetForm();
    console.log('Warehouse updated successfully!');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred.';
  }
});

const handleModalUpdate = (open: boolean) => {
  if (!open) {
    // Close the appropriate modal based on context
    if (uiStore.selectedWarehouseDetails) {
      uiStore.closeEditWarehouseDetailsModal();
    } else {
      uiStore.closeEditWarehouseModal();
    }
    resetForm();
    error.value = null;
  }
};

defineExpose({
  onSubmit,
  isSubmitting,
});
</script>

<template>
  <Dialog :open="uiStore.isEditWarehouseModalOpen" @update:open="handleModalUpdate">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Edit Warehouse</DialogTitle>
        <DialogDescription>
          Update the warehouse name. This will be reflected across all locations and inventory items.
        </DialogDescription>
      </DialogHeader>
      
      <form class="space-y-4" @submit="onSubmit">
        <FormField v-slot="{ componentField }" name="name">
          <FormItem>
            <FormLabel>Warehouse Name</FormLabel>
            <FormControl>
              <Input 
                type="text" 
                placeholder="Main Warehouse" 
                v-bind="componentField" 
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <div v-if="error" class="text-red-500 text-sm">
          {{ error }}
        </div>
      </form>

      <DialogFooter>
        <Button variant="outline" @click="handleModalUpdate(false)">
          Cancel
        </Button>
        <Button 
          type="submit" 
          :disabled="isSubmitting"
          @click="onSubmit"
        >
          <span v-if="isSubmitting">Updating...</span>
          <span v-else>Update Warehouse</span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
