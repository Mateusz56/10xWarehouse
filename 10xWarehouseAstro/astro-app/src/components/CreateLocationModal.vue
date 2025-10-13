<script setup lang="ts">
import { ref, computed } from 'vue';
import { useUiStore } from '@/stores/ui';
import { useWarehouseDetailsStore } from '@/stores/warehouseDetails';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

const uiStore = useUiStore();
const warehouseDetailsStore = useWarehouseDetailsStore();

const name = ref('');
const description = ref('');
const isSubmitting = ref(false);
const error = ref('');

const isValid = computed(() => {
  return name.value.trim().length > 0 && name.value.length <= 100;
});

const warehouseId = computed(() => {
  return warehouseDetailsStore.warehouse?.id || '';
});

function resetForm() {
  name.value = '';
  description.value = '';
  error.value = '';
  isSubmitting.value = false;
}

function handleClose() {
  resetForm();
  uiStore.closeCreateLocationModal();
}

async function handleSubmit() {
  if (!isValid.value || !warehouseId.value) return;

  isSubmitting.value = true;
  error.value = '';

  try {
    await warehouseDetailsStore.createLocation({
      name: name.value.trim(),
      description: description.value.trim() || undefined,
      warehouseId: warehouseId.value
    });

    handleClose();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to create location';
  } finally {
    isSubmitting.value = false;
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault();
    handleSubmit();
  }
}
</script>

<template>
  <Dialog :open="uiStore.isCreateLocationModalOpen" @update:open="handleClose">
    <DialogContent class="sm:max-w-md">
      <DialogHeader>
        <DialogTitle>Create New Location</DialogTitle>
        <DialogDescription>
          Add a new storage location to this warehouse. Location names must be unique within the warehouse.
        </DialogDescription>
      </DialogHeader>

      <div class="space-y-4">
        <!-- Name Field -->
        <div class="space-y-2">
          <Label for="location-name">Name *</Label>
          <Input
            id="location-name"
            v-model="name"
            placeholder="e.g., Aisle 1, Cold Storage, Loading Dock"
            :maxlength="100"
            @keydown="handleKeydown"
            :disabled="isSubmitting"
          />
          <p class="text-xs text-gray-500 dark:text-gray-400">
            {{ name.length }}/100 characters
          </p>
        </div>

        <!-- Description Field -->
        <div class="space-y-2">
          <Label for="location-description">Description</Label>
          <Input
            id="location-description"
            v-model="description"
            placeholder="Optional description for this location"
            :maxlength="500"
            @keydown="handleKeydown"
            :disabled="isSubmitting"
          />
          <p class="text-xs text-gray-500 dark:text-gray-400">
            {{ description.length }}/500 characters
          </p>
        </div>

        <!-- Error Message -->
        <div v-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3">
          <div class="flex items-center">
            <svg class="w-4 h-4 text-red-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p class="text-sm text-red-700 dark:text-red-300">{{ error }}</p>
          </div>
        </div>
      </div>

      <DialogFooter>
        <Button
          variant="outline"
          @click="handleClose"
          :disabled="isSubmitting"
        >
          Cancel
        </Button>
        <Button
          @click="handleSubmit"
          :disabled="!isValid || isSubmitting"
        >
          <svg v-if="isSubmitting" class="animate-spin w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" aria-hidden="true">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ isSubmitting ? 'Creating...' : 'Create Location' }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
