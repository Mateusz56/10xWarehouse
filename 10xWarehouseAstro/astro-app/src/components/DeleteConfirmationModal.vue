<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
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

const router = useRouter();
const uiStore = useUiStore();
const warehouseDetailsStore = useWarehouseDetailsStore();

const confirmationText = ref('');
const isSubmitting = ref(false);
const error = ref('');

const isLocationDelete = computed(() => {
  return uiStore.isDeleteLocationModalOpen && uiStore.selectedLocation;
});

const isWarehouseDelete = computed(() => {
  return uiStore.isDeleteWarehouseModalOpen && (uiStore.selectedWarehouseDetails || uiStore.selectedWarehouse);
});

const selectedItem = computed(() => {
  if (isLocationDelete.value) {
    return uiStore.selectedLocation;
  } else if (isWarehouseDelete.value) {
    return uiStore.selectedWarehouseDetails || uiStore.selectedWarehouse;
  }
  return null;
});

const itemName = computed(() => {
  return selectedItem.value?.name || '';
});

const confirmationPlaceholder = computed(() => {
  return isLocationDelete.value ? 'location name' : 'warehouse name';
});

const isValid = computed(() => {
  // For locations, no name confirmation required
  if (isLocationDelete.value) {
    return true;
  }
  // For warehouses, require name confirmation
  return confirmationText.value.trim() === itemName.value;
});

const warningMessage = computed(() => {
  if (isLocationDelete.value) {
    return 'This action cannot be undone. The location will be permanently removed from the warehouse.';
  } else {
    return 'This action cannot be undone. The warehouse and all its locations will be permanently removed. Note: The warehouse can only be deleted if all locations are empty (no inventory).';
  }
});

// Watch for changes in selected item and reset form
watch(selectedItem, () => {
  confirmationText.value = '';
  error.value = '';
  isSubmitting.value = false;
}, { immediate: true });

function resetForm() {
  confirmationText.value = '';
  error.value = '';
  isSubmitting.value = false;
}

function handleClose() {
  resetForm();
  if (isLocationDelete.value) {
    uiStore.closeDeleteLocationModal();
  } else if (isWarehouseDelete.value) {
    // Check which warehouse was selected to close the appropriate modal
    if (uiStore.selectedWarehouseDetails) {
      uiStore.closeDeleteWarehouseDetailsModal();
    } else {
      uiStore.closeDeleteWarehouseModal();
    }
  }
}

async function handleConfirm() {
  if (!isValid.value || !selectedItem.value) return;

  isSubmitting.value = true;
  error.value = '';

  try {
    if (isLocationDelete.value) {
      await warehouseDetailsStore.deleteLocation(selectedItem.value.id);
    } else if (isWarehouseDelete.value) {
      // Use the appropriate store based on which warehouse was selected
      if (uiStore.selectedWarehouseDetails) {
        await warehouseDetailsStore.deleteWarehouse(selectedItem.value.id);
        // Redirect to warehouses list after successful deletion
        router.push('/warehouses');
      } else {
        // Import warehouseStore for this case
        const { useWarehouseStore } = await import('@/stores/warehouse');
        const warehouseStore = useWarehouseStore();
        await warehouseStore.deleteWarehouse(selectedItem.value.id);
        // No redirect needed as we're already on the warehouses list
      }
    }

    handleClose();
  } catch (err) {
    error.value = err instanceof Error ? err.message : `Failed to delete ${isLocationDelete.value ? 'location' : 'warehouse'}`;
  } finally {
    isSubmitting.value = false;
  }
}

function handleKeydown(event: KeyboardEvent) {
  // Only handle Enter key for warehouses (which have confirmation input)
  if (event.key === 'Enter' && !event.shiftKey && !isLocationDelete.value) {
    event.preventDefault();
    handleConfirm();
  }
}
</script>

<template>
  <Dialog 
    :open="uiStore.isDeleteLocationModalOpen || uiStore.isDeleteWarehouseModalOpen" 
    @update:open="handleClose"
  >
    <DialogContent class="sm:max-w-md">
      <DialogHeader>
        <DialogTitle class="text-red-600 dark:text-red-400">
          Delete {{ isLocationDelete ? 'Location' : 'Warehouse' }}
        </DialogTitle>
        <DialogDescription>
          {{ warningMessage }}
        </DialogDescription>
      </DialogHeader>

      <div class="space-y-4">
        <!-- Warning Box -->
        <div class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
          <div class="flex items-start">
            <svg class="w-5 h-5 text-red-400 mr-3 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.268 19.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
            <div>
              <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                Are you sure you want to delete "{{ itemName }}"?
              </h3>
              <p class="text-sm text-red-700 dark:text-red-300 mt-1">
                {{ warningMessage }}
              </p>
            </div>
          </div>
        </div>

        <!-- Confirmation Input (only for warehouses) -->
        <div v-if="!isLocationDelete" class="space-y-2">
          <Label for="confirmation-input">
            Type <strong>{{ itemName }}</strong> to confirm deletion
          </Label>
          <Input
            id="confirmation-input"
            v-model="confirmationText"
            :placeholder="`Enter ${confirmationPlaceholder}`"
            @keydown="handleKeydown"
            :disabled="isSubmitting"
            class="border-red-200 dark:border-red-800 focus:border-red-400 dark:focus:border-red-600"
          />
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
          variant="destructive"
          @click="handleConfirm"
          :disabled="!isValid || isSubmitting"
        >
          <svg v-if="isSubmitting" class="animate-spin w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" aria-hidden="true">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ isSubmitting ? 'Deleting...' : (isLocationDelete ? 'Delete' : `Delete Warehouse`) }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>