<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const isInitializing = ref(true)

onMounted(async () => {
  // Wait for auth to be initialized if it hasn't been yet
  if (!authStore.isInitialized) {
    await authStore.initializeAuth()
  }
  
  isInitializing.value = false
  
  if (!authStore.isAuthenticated) {
    window.location.href = '/login'
  }
})
</script>

<template>
  <div v-if="isInitializing" class="flex items-center justify-center min-h-screen">
    <div class="text-lg">Loading...</div>
  </div>
  <slot v-else-if="authStore.isAuthenticated" />
  <div v-else class="flex items-center justify-center min-h-screen">
    <div class="text-center">
      <h2 class="text-2xl font-bold mb-4">Authentication Required</h2>
      <p class="text-gray-600 mb-4">Please sign in to access this page.</p>
      <a href="/login" class="inline-block bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
        Go to Login
      </a>
    </div>
  </div>
</template>
