<script setup lang="ts">
import { ref } from 'vue';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import CreateOrganizationForm from './CreateOrganizationForm.vue';
import { Button } from '@/components/ui/button';
import { useUiStore } from '@/stores/ui';

const uiStore = useUiStore();
const form = ref<InstanceType<typeof CreateOrganizationForm> | null>(null);

function handleOpenChange(open: boolean) {
  if (!open) {
    uiStore.closeCreateOrganizationModal();
  }
}
</script>

<template>
  <Dialog :open="uiStore.isCreateOrganizationModalOpen" @update:open="handleOpenChange">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>Create a new organization</DialogTitle>
        <DialogDescription>
          Enter a name for your new organization.
        </DialogDescription>
      </DialogHeader>

      <CreateOrganizationForm ref="form" />

      <DialogFooter>
        <Button variant="outline" @click="uiStore.closeCreateOrganizationModal()">
          Cancel
        </Button>
        <Button
          @click="form?.onSubmit"
          :disabled="form?.isSubmitting"
        >
          {{ form?.isSubmitting ? 'Creating...' : 'Create' }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
