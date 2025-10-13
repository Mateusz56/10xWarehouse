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
import { useWarehouseStore } from '@/stores/warehouse';

const uiStore = useUiStore();
const warehouseStore = useWarehouseStore();
const error = ref<string | null>(null);

const formSchema = toTypedSchema(z.object({
  name: z.string()
    .min(1, 'Name is required.')
    .max(100, 'Name must be at most 100 characters.')
    .regex(/^[a-zA-Z0-9\s\-_]+$/, 'Name can only contain letters, numbers, spaces, hyphens, and underscores.'),
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
    
    await warehouseStore.createWarehouse({
      ...values,
      organizationId: orgStore.currentOrganizationId
    });
    uiStore.closeCreateWarehouseModal();
    resetForm();
    console.log('Warehouse created successfully!');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred.';
  }
});

const handleModalUpdate = (open: boolean) => {
  if (!open) {
    uiStore.closeCreateWarehouseModal();
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
  <Dialog :open="uiStore.isCreateWarehouseModalOpen" @update:open="handleModalUpdate">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Create Warehouse</DialogTitle>
        <DialogDescription>
          Enter a name for your new warehouse. This will be used to organize your inventory locations.
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
        <Button variant="outline" @click="uiStore.closeCreateWarehouseModal()">
          Cancel
        </Button>
        <Button 
          type="submit" 
          :disabled="isSubmitting"
          @click="onSubmit"
        >
          <span v-if="isSubmitting">Creating...</span>
          <span v-else>Create Warehouse</span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
