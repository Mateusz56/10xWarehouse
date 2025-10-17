<script setup lang="ts">
import { computed } from 'vue';
import { Card, CardHeader, CardContent, CardFooter } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import PasswordStrengthIndicator from './PasswordStrengthIndicator.vue';
import type { PasswordChangeFormData, PasswordStrength } from '@/types/dto';

const props = defineProps<{
  loading: boolean;
  error: string | null;
  success: string | null;
  formData: PasswordChangeFormData;
  isValid: boolean;
  passwordValidation: { isValid: boolean; message?: string };
  passwordConfirmationValidation: { isValid: boolean; message?: string };
  passwordStrength: { score: number; label: string; color: string; feedback: string[] };
}>();

const emit = defineEmits<{
  'change-password': [];
  reset: [];
}>();

const currentPassword = computed({
  get: () => props.formData.currentPassword,
  set: (value: string) => {
    props.formData.currentPassword = value;
  }
});

const newPassword = computed({
  get: () => props.formData.newPassword,
  set: (value: string) => {
    props.formData.newPassword = value;
  }
});

const confirmPassword = computed({
  get: () => props.formData.confirmPassword,
  set: (value: string) => {
    props.formData.confirmPassword = value;
  }
});

const passwordStrength = computed((): PasswordStrength => {
  const password = newPassword.value;
  let score = 0;
  const feedback: string[] = [];
  
  if (password.length >= 6) score++;
  else feedback.push('Use at least 6 characters');
  
  if (password.length >= 8) score++;
  else if (password.length > 0) feedback.push('Use at least 8 characters for better security');
  
  if (/[A-Z]/.test(password)) score++;
  else if (password.length > 0) feedback.push('Include uppercase letters');
  
  if (/[0-9]/.test(password)) score++;
  else if (password.length > 0) feedback.push('Include numbers');
  
  if (/[^A-Za-z0-9]/.test(password)) score++;
  else if (password.length > 0) feedback.push('Include special characters');
  
  const labels = ['Very Weak', 'Weak', 'Fair', 'Good', 'Strong'];
  const colors = ['red', 'orange', 'yellow', 'blue', 'green'];
  
  return {
    score: Math.min(score, 4),
    label: labels[Math.min(score, 4)] as PasswordStrength['label'],
    color: colors[Math.min(score, 4)] as PasswordStrength['color'],
    feedback
  };
});

const passwordsMatch = computed(() => {
  return newPassword.value === confirmPassword.value || confirmPassword.value.length === 0;
});
</script>

<template>
  <Card>
    <CardHeader>
      <h2 class="text-xl font-semibold text-foreground">Change Password</h2>
      <p class="text-sm text-muted-foreground">Update your password to keep your account secure</p>
    </CardHeader>
    
    <CardContent>
      <div class="space-y-4">
        <div class="space-y-2">
          <Label for="currentPassword">Current Password</Label>
          <Input
            id="currentPassword"
            v-model="currentPassword"
            type="password"
            placeholder="Enter your current password"
          />
        </div>
        
        <div class="space-y-2">
          <Label for="newPassword">New Password</Label>
          <Input
            id="newPassword"
            v-model="newPassword"
            type="password"
            placeholder="Enter your new password"
            :class="{ 'border-red-500': newPassword.length > 0 && !passwordValidation.isValid }"
          />
          <PasswordStrengthIndicator 
            v-if="newPassword.length > 0"
            :password="newPassword"
            :strength="passwordStrength"
          />
          <p v-if="newPassword.length > 0 && !passwordValidation.isValid" class="text-sm text-red-500">
            {{ passwordValidation.message }}
          </p>
          <div v-if="newPassword.length > 0 && passwordStrength.feedback.length > 0" class="text-xs text-gray-600">
            <p class="font-medium">Suggestions:</p>
            <ul class="list-disc list-inside mt-1">
              <li v-for="feedback in passwordStrength.feedback" :key="feedback">{{ feedback }}</li>
            </ul>
          </div>
        </div>
        
        <div class="space-y-2">
          <Label for="confirmPassword">Confirm New Password</Label>
          <Input
            id="confirmPassword"
            v-model="confirmPassword"
            type="password"
            placeholder="Confirm your new password"
            :class="{ 'border-red-500': confirmPassword.length > 0 && !passwordConfirmationValidation.isValid }"
          />
          <p v-if="confirmPassword.length > 0 && !passwordConfirmationValidation.isValid" class="text-sm text-red-500">
            {{ passwordConfirmationValidation.message }}
          </p>
        </div>
      </div>
    </CardContent>
    
    <CardFooter class="flex justify-between">
      <div class="flex space-x-2">
        <Button
          @click="emit('change-password')"
          :disabled="!isValid || loading"
          :class="{ 'opacity-50 cursor-not-allowed': !isValid || loading }"
        >
          <span v-if="loading">Changing Password...</span>
          <span v-else>Change Password</span>
        </Button>
        
        <Button
          variant="outline"
          @click="emit('reset')"
          :disabled="loading"
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
