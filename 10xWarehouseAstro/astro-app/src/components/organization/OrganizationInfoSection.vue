<template>
  <div class="bg-card shadow rounded-lg border border-border">
    <div class="px-4 py-5 sm:p-6">
      <h3 class="text-lg leading-6 font-medium text-card-foreground mb-4">
        Organization Information
      </h3>
      
      <div v-if="isEditing" class="space-y-4">
        <div class="space-y-2">
          <Label for="organization-name">Organization Name</Label>
          <Input
            id="organization-name"
            v-model="editingName"
            type="text"
            placeholder="Enter organization name"
            :class="{ 'border-destructive': nameError }"
          />
          <p v-if="nameError" class="text-sm text-destructive">{{ nameError }}</p>
        </div>
        
        <div class="flex justify-end space-x-3">
          <Button
            variant="outline"
            @click="cancelEditing"
          >
            Cancel
          </Button>
          <Button
            @click="saveChanges"
            :disabled="loading || !editingName.trim()"
          >
            {{ loading ? 'Saving...' : 'Save' }}
          </Button>
        </div>
      </div>
      
      <div v-else class="flex items-center justify-between">
        <div>
          <h4 class="text-xl font-semibold text-card-foreground">{{ organization?.name || 'Loading...' }}</h4>
          <p class="text-sm text-muted-foreground">Organization name</p>
        </div>
        <Button
          @click="startEditing"
        >
          Edit
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { OrganizationVM, UpdateOrganizationRequestDto } from '@/types/dto';

interface Props {
  organization: OrganizationVM | null;
  loading: boolean;
  error: string | null;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  'update-organization': [data: UpdateOrganizationRequestDto];
}>();

const isEditing = ref(false);
const editingName = ref('');
const nameError = ref('');

const startEditing = () => {
  isEditing.value = true;
  editingName.value = props.organization?.name || '';
  nameError.value = '';
};

const cancelEditing = () => {
  isEditing.value = false;
  editingName.value = '';
  nameError.value = '';
};

const saveChanges = async () => {
  if (!editingName.value.trim()) {
    nameError.value = 'Organization name is required';
    return;
  }
  
  if (editingName.value.length > 100) {
    nameError.value = 'Organization name must be 100 characters or less';
    return;
  }
  
  nameError.value = '';
  
  try {
    await emit('update-organization', { name: editingName.value.trim() });
    isEditing.value = false;
  } catch (error) {
    nameError.value = 'Failed to update organization name';
  }
};
</script>
