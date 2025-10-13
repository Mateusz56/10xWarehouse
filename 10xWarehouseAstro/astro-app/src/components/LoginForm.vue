<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const authStore = useAuthStore()
const email = ref('')
const password = ref('')
const error = ref('')

async function handleLogin() {
  error.value = ''
  
  if (!email.value || !password.value) {
    error.value = 'Please fill in all fields'
    return
  }

  const { error: signInError } = await authStore.signIn(email.value, password.value)
  
  if (signInError) {
    error.value = signInError.message || 'Login failed'
  } else {
    // Redirect to main page after successful login
    window.location.href = '/'
  }
}
</script>

<template>
  <div class="w-full max-w-md mx-auto">
    <form @submit.prevent="handleLogin" class="space-y-4">
      <div>
        <Label for="email">Email</Label>
        <Input
          id="email"
          v-model="email"
          type="email"
          required
          placeholder="Enter your email"
        />
      </div>
      
      <div>
        <Label for="password">Password</Label>
        <Input
          id="password"
          v-model="password"
          type="password"
          required
          placeholder="Enter your password"
        />
      </div>
      
      <Button type="submit" class="w-full" :disabled="authStore.loading">
        {{ authStore.loading ? 'Signing in...' : 'Sign In' }}
      </Button>
      
      <div v-if="error" class="text-red-500 text-sm">
        {{ error }}
      </div>
    </form>
  </div>
</template>
