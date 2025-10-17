<script setup lang="ts">
import { onMounted } from 'vue';
import { Card, CardHeader, CardContent, CardFooter } from '@/components/ui/card';
import DisplayNameCard from './DisplayNameCard.vue';
import PasswordChangeCard from './PasswordChangeCard.vue';
import AccountInfoCard from './AccountInfoCard.vue';
import { useProfilePage } from '@/composables/useProfilePage';

const {
  user,
  accountCreatedAt,
  displayNameForm,
  passwordChangeForm,
  displayNameLoading,
  passwordChangeLoading,
  displayNameError,
  passwordChangeError,
  displayNameSuccess,
  passwordChangeSuccess,
  isDisplayNameFormValid,
  isPasswordChangeFormValid,
  hasDisplayNameChanges,
  displayNameValidation,
  passwordValidation,
  passwordConfirmationValidation,
  passwordStrength,
  initializeUser,
  updateDisplayName,
  changePassword,
  resetDisplayNameForm,
  resetPasswordChangeForm,
  clearMessages
} = useProfilePage();

onMounted(() => {
  initializeUser();
});
</script>

<template>
  <div class="container mx-auto px-4 py-8 max-w-4xl">
    <!-- Profile Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-foreground">Profile Settings</h1>
      <p class="text-muted-foreground mt-2">Manage your account settings and preferences</p>
    </div>

    <!-- Debug Info -->
    <div v-if="!user" class="mb-4 p-4 bg-yellow-100 border border-yellow-400 rounded">
      <p class="text-yellow-800">Loading user data... Check console for details.</p>
    </div>

    <!-- Profile Cards -->
    <div class="space-y-6">
      <!-- Display Name Card -->
      <DisplayNameCard 
        :user="user || { id: '', email: '', displayName: '' }"
        :loading="displayNameLoading"
        :error="displayNameError"
        :success="displayNameSuccess"
        :form-data="displayNameForm"
        :is-valid="isDisplayNameFormValid"
        :has-changes="hasDisplayNameChanges"
        :validation="displayNameValidation"
        @update="updateDisplayName"
        @reset="resetDisplayNameForm"
      />

      <!-- Password Change Card -->
      <PasswordChangeCard 
        :loading="passwordChangeLoading"
        :error="passwordChangeError"
        :success="passwordChangeSuccess"
        :form-data="passwordChangeForm"
        :is-valid="isPasswordChangeFormValid"
        :password-validation="passwordValidation"
        :password-confirmation-validation="passwordConfirmationValidation"
        :password-strength="passwordStrength"
        @change-password="changePassword"
        @reset="resetPasswordChangeForm"
      />

      <!-- Account Info Card -->
      <AccountInfoCard 
        v-if="user"
        :user="user"
        :account-created-at="accountCreatedAt || new Date().toISOString()"
      />
    </div>
  </div>
</template>
