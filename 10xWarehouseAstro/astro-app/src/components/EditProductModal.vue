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
import { useProductStore } from '@/stores/product';

const uiStore = useUiStore();
const productStore = useProductStore();
const error = ref<string | null>(null);

const formSchema = toTypedSchema(z.object({
  name: z.string()
    .min(1, 'Name is required.')
    .max(100, 'Name must be at most 100 characters.'),
  barcode: z.string()
    .max(50, 'Barcode must be at most 50 characters.')
    .optional()
    .or(z.literal('')),
  description: z.string()
    .max(500, 'Description must be at most 500 characters.')
    .optional()
    .or(z.literal('')),
  lowStockThreshold: z.coerce.number()
    .min(0, 'Low stock threshold must be non-negative.')
    .optional()
    .or(z.literal('')),
}));

const { handleSubmit, isSubmitting, resetForm, setFieldValue } = useForm({
  validationSchema: formSchema,
});

// Watch for selected product changes and populate form
watch(() => uiStore.selectedProduct, (product) => {
  if (product) {
    setFieldValue('name', product.name);
    setFieldValue('barcode', product.barcode || '');
    setFieldValue('description', product.description || '');
    setFieldValue('lowStockThreshold', product.lowStockThreshold || '');
  }
}, { immediate: true });

const onSubmit = handleSubmit(async (values) => {
  const selectedProduct = uiStore.selectedProduct;
  if (!selectedProduct) return;
  
  error.value = null;
  try {
    // Clean up empty strings to undefined
    const cleanValues = {
      name: values.name,
      barcode: values.barcode || undefined,
      description: values.description || undefined,
      lowStockThreshold: values.lowStockThreshold || undefined,
    };
    
    await productStore.updateProduct(selectedProduct.id, cleanValues);
    uiStore.closeEditProductModal();
    resetForm();
    console.log('Product updated successfully!');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred.';
  }
});

const handleModalUpdate = (open: boolean) => {
  if (!open) {
    uiStore.closeEditProductModal();
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
  <Dialog :open="uiStore.isEditProductModalOpen" @update:open="handleModalUpdate">
    <DialogContent class="sm:max-w-[500px]">
      <DialogHeader>
        <DialogTitle>Edit Product Template</DialogTitle>
        <DialogDescription>
          Update the product template details. Changes will be reflected in all inventory items using this template.
        </DialogDescription>
      </DialogHeader>
      
      <form class="space-y-4" @submit="onSubmit">
        <FormField v-slot="{ componentField }" name="name">
          <FormItem>
            <FormLabel>Product Name *</FormLabel>
            <FormControl>
              <Input 
                type="text" 
                placeholder="Enter product name" 
                v-bind="componentField" 
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField v-slot="{ componentField }" name="barcode">
          <FormItem>
            <FormLabel>Barcode</FormLabel>
            <FormControl>
              <Input 
                type="text" 
                placeholder="Enter barcode (optional)" 
                v-bind="componentField" 
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField v-slot="{ componentField }" name="description">
          <FormItem>
            <FormLabel>Description</FormLabel>
            <FormControl>
              <Input 
                type="text" 
                placeholder="Enter description (optional)" 
                v-bind="componentField" 
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField v-slot="{ componentField }" name="lowStockThreshold">
          <FormItem>
            <FormLabel>Low Stock Threshold</FormLabel>
            <FormControl>
              <Input 
                type="number" 
                placeholder="Enter threshold (optional)" 
                min="0"
                step="1"
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
        <Button variant="outline" @click="uiStore.closeEditProductModal()">
          Cancel
        </Button>
        <Button 
          type="submit" 
          :disabled="isSubmitting"
          @click="onSubmit"
        >
          <span v-if="isSubmitting">Updating...</span>
          <span v-else>Update Product</span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
