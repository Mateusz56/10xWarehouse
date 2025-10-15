<script setup lang="ts">
import { ref } from 'vue';
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

const { handleSubmit, isSubmitting, resetForm } = useForm({
  validationSchema: formSchema,
});

const onSubmit = handleSubmit(async (values) => {
  error.value = null;
  try {
    const { useOrganizationStore } = await import('@/stores/organization');
    const orgStore = useOrganizationStore();
    
    if (!orgStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }
    
    // Clean up empty strings to undefined
    const cleanValues = {
      name: values.name,
      barcode: values.barcode || undefined,
      description: values.description || undefined,
      lowStockThreshold: values.lowStockThreshold || undefined,
    };
    
    await productStore.createProduct(cleanValues);
    uiStore.closeCreateProductModal();
    resetForm();
    console.log('Product created successfully!');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred.';
  }
});

const handleModalUpdate = (open: boolean) => {
  if (!open) {
    uiStore.closeCreateProductModal();
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
  <Dialog :open="uiStore.isCreateProductModalOpen" @update:open="handleModalUpdate">
    <DialogContent class="sm:max-w-[500px]">
      <DialogHeader>
        <DialogTitle>Create Product Template</DialogTitle>
        <DialogDescription>
          Enter the details for your new product template. This will be used to create inventory items.
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
        <Button variant="outline" @click="uiStore.closeCreateProductModal()">
          Cancel
        </Button>
        <Button 
          type="submit" 
          :disabled="isSubmitting"
          @click="onSubmit"
        >
          <span v-if="isSubmitting">Creating...</span>
          <span v-else>Create Product</span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
