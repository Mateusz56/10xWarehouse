import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { supabase } from '@/lib/supabase'
import type { User } from '@supabase/supabase-js'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const session = ref<any>(null)
  const loading = ref(false)
  const isInitialized = ref(false)

  const isAuthenticated = computed(() => !!user.value)

  async function signUp(email: string, password: string, displayName?: string) {
    loading.value = true
    try {
      const { data, error } = await supabase.auth.signUp({
        email,
        password,
        options: {
          data: {
            display_name: displayName
          }
        }
      })
      
      if (error) throw error
      return { data, error: null }
    } catch (error) {
      return { data: null, error }
    } finally {
      loading.value = false
    }
  }

  async function signIn(email: string, password: string) {
    loading.value = true
    try {
      const { data, error } = await supabase.auth.signInWithPassword({
        email,
        password
      })
      
      if (error) throw error
      return { data, error: null }
    } catch (error) {
      return { data: null, error }
    } finally {
      loading.value = false
    }
  }

  async function signOut() {
    loading.value = true
    try {
      const { error } = await supabase.auth.signOut()
      if (error) throw error
      
      user.value = null
      session.value = null
    } catch (error) {
      console.error('Error signing out:', error)
    } finally {
      loading.value = false
    }
  }

  async function initializeAuth() {
    if (isInitialized.value) {
      return // Already initialized
    }

    try {
      const { data: { session: currentSession } } = await supabase.auth.getSession()
      
      if (currentSession) {
        session.value = currentSession
        user.value = currentSession.user
      }

      // Listen for auth changes - simplified approach
      supabase.auth.onAuthStateChange((event, newSession) => {
        session.value = newSession
        user.value = newSession?.user ?? null
        
        if (event === 'SIGNED_OUT') {
          user.value = null
          session.value = null
        }
      })

      isInitialized.value = true
    } catch (error) {
      console.error('Error initializing auth:', error)
    }
  }

  function getAccessToken() {
    return session.value?.access_token
  }

  return {
    user,
    session,
    loading,
    isInitialized,
    isAuthenticated,
    signUp,
    signIn,
    signOut,
    initializeAuth,
    getAccessToken
  }
})
