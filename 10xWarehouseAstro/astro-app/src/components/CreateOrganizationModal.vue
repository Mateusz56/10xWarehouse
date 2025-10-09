<script setup lang="ts">
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import CreateOrganizationForm from './CreateOrganizationForm.vue';
import { useUiStore } from '@/stores/ui';
import { Button } from '@/components/ui/button';
import { ref } from 'vue';

const uiStore = useUiStore();
const form = ref<InstanceType<typeof CreateOrganizationForm> | null>(null);

const handleModalUpdate = (open: boolean) => {
  if (!open) {
    uiStore.closeCreateOrganizationModal();
  }
};
</script>

<template>
  <Dialog :open="uiStore.isCreateOrganizationModalOpen" @update:open="handleModalUpdate">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle>Create Organization</DialogTitle>
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
          type="submit" 
          :disabled="form?.isSubmitting"
          @click="form?.onSubmit"
        >
          <span v-if="form?.isSubmitting">Creating...</span>
          <span v-else>Create</span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
