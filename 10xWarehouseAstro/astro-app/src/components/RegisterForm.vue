<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const authStore = useAuthStore()
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const displayName = ref('')
const createOrganization = ref(false)
const organizationName = ref('')
const error = ref('')
const success = ref('')

async function handleRegister() {
  error.value = ''
  success.value = ''
  
  // Validation
  if (!email.value || !password.value || !displayName.value) {
    error.value = 'Please fill in all required fields'
    return
  }

  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match'
    return
  }

  if (password.value.length < 6) {
    error.value = 'Password must be at least 6 characters long'
    return
  }

  if (createOrganization.value && !organizationName.value) {
    error.value = 'Organization name is required when creating an organization'
    return
  }

  try {
    // Call backend API directly for registration
    const { data: backendData, error: backendError } = await authStore.registerWithBackend(
      email.value,
      password.value,
      displayName.value,
      createOrganization.value,
      organizationName.value
    )

    if (backendError) {
      error.value = backendError.msg || 'Registration failed'
      return
    }

    success.value = 'Registration successful! You will be redirected to the login page in 2 seconds.'
    // Clear form
    email.value = ''
    password.value = ''
    confirmPassword.value = ''
    displayName.value = ''
    createOrganization.value = false
    organizationName.value = ''
    
    // Redirect to main page after successful registration
    setTimeout(() => {
      window.location.href = '/'
    }, 2000) // 2 second delay to show success message
  } catch (err) {
    error.value = 'An unexpected error occurred during registration'
    console.error('Registration error:', err)
  }
}
</script>

<template>
  <div class="w-full max-w-md mx-auto">
    <form @submit.prevent="handleRegister" class="space-y-4">
      <div>
        <Label for="displayName">Display Name *</Label>
        <Input
          id="displayName"
          v-model="displayName"
          type="text"
          required
          placeholder="Enter your display name"
        />
      </div>

      <div>
        <Label for="email">Email *</Label>
        <Input
          id="email"
          v-model="email"
          type="email"
          required
          placeholder="Enter your email"
        />
      </div>
      
      <div>
        <Label for="password">Password *</Label>
        <Input
          id="password"
          v-model="password"
          type="password"
          required
          placeholder="Enter your password (min 6 characters)"
        />
      </div>

      <div>
        <Label for="confirmPassword">Confirm Password *</Label>
        <Input
          id="confirmPassword"
          v-model="confirmPassword"
          type="password"
          required
          placeholder="Confirm your password"
        />
      </div>

      <div class="flex items-center space-x-2">
        <input 
          id="createOrganization" 
          v-model="createOrganization"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <Label for="createOrganization" class="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
          Create my first organization
        </Label>
      </div>

      <div v-if="createOrganization">
        <Label for="organizationName">Organization Name *</Label>
        <Input
          id="organizationName"
          v-model="organizationName"
          type="text"
          required
          placeholder="Enter organization name"
        />
      </div>
      
      <Button type="submit" class="w-full" :disabled="authStore.loading">
        {{ authStore.loading ? 'Creating account...' : 'Create Account' }}
      </Button>
      
      <div v-if="error" class="text-red-500 text-sm">
        {{ error }}
      </div>

      <div v-if="success" class="text-green-500 text-sm">
        {{ success }}
      </div>

      <div class="text-center text-sm">
        <span class="text-gray-600">Already have an account? </span>
        <a href="/login" class="text-blue-600 hover:text-blue-500 font-medium">
          Sign in
        </a>
      </div>
    </form>
  </div>
</template>
