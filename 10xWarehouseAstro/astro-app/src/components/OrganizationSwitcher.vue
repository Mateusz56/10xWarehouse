<script setup lang="ts">
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { OrganizationVM } from '@/types/dto';

defineProps<{
  organizations: OrganizationVM[];
  currentOrganizationId: string | null;
  loading?: boolean;
}>();

const emit = defineEmits(['select']);

function handleSelect(value: any) {
  if (typeof value === 'string') {
    emit('select', value);
  }
}
</script>

<template>
  <div class="w-full">
    <Select 
      :model-value="currentOrganizationId ?? ''" 
      @update:model-value="handleSelect"
      :disabled="organizations.length === 0 || loading"
    >
      <SelectTrigger class="w-full">
        <SelectValue 
          :placeholder="loading ? 'Loading organizations...' : (organizations.length === 0 ? 'No organizations available' : 'Select an organization')" 
        />
      </SelectTrigger>
      <SelectContent v-if="organizations.length > 0 && !loading">
        <SelectGroup>
          <SelectLabel>Organizations</SelectLabel>
          <SelectItem
            v-for="org in organizations"
            :key="org.id"
            :value="org.id"
          >
            {{ org.name }}
          </SelectItem>
        </SelectGroup>
      </SelectContent>
    </Select>
    
    <!-- Show helpful message when no organizations -->
    <p 
      v-if="organizations.length === 0 && !loading" 
      class="text-sm text-gray-500 mt-1"
    >
      Create your first organization to get started
    </p>
    
    <!-- Show loading message -->
    <p 
      v-if="loading" 
      class="text-sm text-gray-500 mt-1"
    >
      Loading organizations...
    </p>
  </div>
</template>
