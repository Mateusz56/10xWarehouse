<script setup lang="ts">
import { ref } from 'vue';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import * as z from 'zod';

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
import { useOrganizationStore } from '@/stores/organization';
import type { OrganizationDto } from '@/types/dto';

const uiStore = useUiStore();
const organizationStore = useOrganizationStore();
const error = ref<string | null>(null);

const formSchema = toTypedSchema(z.object({
  name: z.string().min(3, 'Name must be at least 3 characters.').max(100, 'Name must be at most 100 characters.'),
}));

const { handleSubmit, isSubmitting, resetForm } = useForm({
  validationSchema: formSchema,
});

const onSubmit = handleSubmit(async (values) => {
  error.value = null;
  try {
    await organizationStore.createOrganization(values);
    uiStore.closeCreateOrganizationModal();
    resetForm();
    alert('Organization created!'); // TODO: Replace with a toast component
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unexpected error occurred.';
  }
});

defineExpose({
  onSubmit,
  isSubmitting,
});
</script>

<template>
  <form class="space-y-4" @submit="onSubmit">
    <FormField v-slot="{ componentField }" name="name">
      <FormItem>
        <FormLabel>Organization Name</FormLabel>
        <FormControl>
          <Input type="text" placeholder="Acme Inc." v-bind="componentField" />
        </FormControl>
        <FormMessage />
      </FormItem>
    </FormField>

    <div v-if="error" class="text-red-500 text-sm">
      {{ error }}
    </div>

    <!-- The button is now in the modal footer -->
  </form>
</template>
