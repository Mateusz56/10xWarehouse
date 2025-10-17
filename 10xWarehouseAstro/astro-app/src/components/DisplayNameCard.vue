<script setup lang="ts">
import { computed } from 'vue';
import { Card, CardHeader, CardContent, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { UserProfileDto, DisplayNameFormData } from '@/types/dto';

const props = defineProps<{
  user: UserProfileDto | null;
  loading: boolean;
  error: string | null;
  success: string | null;
  formData: DisplayNameFormData;
  isValid: boolean;
  hasChanges: boolean;
  validation: { isValid: boolean; message?: string };
}>();

const emit = defineEmits<{
  update: [];
  reset: [];
}>();

const displayName = computed({
  get: () => props.formData.displayName,
  set: (value: string) => {
    props.formData.displayName = value;
  }
});

const isUserLoaded = computed(() => props.user && props.user.id);
</script>

<template>
  <Card>
    <CardHeader>
      <h2 class="text-xl font-semibold text-foreground">Display Name</h2>
      <p class="text-sm text-muted-foreground">Update your display name that will be shown to other users</p>
    </CardHeader>
    
    <CardContent>
      <div v-if="!isUserLoaded" class="space-y-4">
        <div class="space-y-2">
          <Label for="displayName">Display Name</Label>
          <div class="h-10 bg-gray-100 rounded-md animate-pulse"></div>
          <p class="text-sm text-gray-500">Loading user data...</p>
        </div>
      </div>
      <div v-else class="space-y-4">
        <div class="space-y-2">
          <Label for="displayName">Display Name</Label>
          <Input
            id="displayName"
            v-model="displayName"
            type="text"
            placeholder="Enter your display name"
            :class="{ 'border-red-500': displayName.length > 0 && !validation.isValid }"
          />
          <p v-if="displayName.length > 0 && !validation.isValid" class="text-sm text-red-500">
            {{ validation.message }}
          </p>
        </div>
      </div>
    </CardContent>
    
    <CardFooter class="flex justify-between">
      <div class="flex space-x-2">
        <Button
          @click="emit('update')"
          :disabled="!isUserLoaded || !isValid || !hasChanges || loading"
          :class="{ 'opacity-50 cursor-not-allowed': !isUserLoaded || !isValid || !hasChanges || loading }"
        >
          <span v-if="loading">Saving...</span>
          <span v-else-if="!isUserLoaded">Loading...</span>
          <span v-else>Save Changes</span>
        </Button>
        
        <Button
          variant="outline"
          @click="emit('reset')"
          :disabled="!isUserLoaded || !hasChanges || loading"
        >
          Reset
        </Button>
      </div>
      
      <div v-if="error" class="text-sm text-red-500">
        {{ error }}
      </div>
      
      <div v-else-if="success" class="text-sm text-green-500">
        {{ success }}
      </div>
    </CardFooter>
  </Card>
</template>
