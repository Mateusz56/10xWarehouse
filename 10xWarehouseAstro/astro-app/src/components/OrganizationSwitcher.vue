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
}>();

const emit = defineEmits(['select']);

function handleSelect(orgId: string) {
  emit('select', orgId);
}
</script>

<template>
  <Select :model-value="currentOrganizationId ?? ''" @update:model-value="handleSelect">
    <SelectTrigger>
      <SelectValue placeholder="Select an organization" />
    </SelectTrigger>
    <SelectContent>
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
</template>
