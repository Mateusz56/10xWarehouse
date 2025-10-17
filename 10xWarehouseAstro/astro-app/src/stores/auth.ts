import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { supabase } from '@/lib/supabase'
import { api } from '@/lib/api'
import type { User } from '@supabase/supabase-js'

const supabaseUrl = import.meta.env.PUBLIC_SUPABASE_URL

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const session = ref<any>(null)
  const loading = ref(false)
  const isInitialized = ref(false)
  const isPasswordChangeInProgress = ref(false)

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

  async function registerWithBackend(email: string, password: string, displayName: string, createOrganization: boolean, organizationName?: string) {
    loading.value = true
    try {
      const data = await api.register({
        email,
        password,
        displayName,
        createOrganization,
        organizationName
      })
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

  async function clearSession() {
    // Set flag to prevent AuthGuard from redirecting immediately
    isPasswordChangeInProgress.value = true
    
    // Clear local session data without making API calls
    user.value = null
    session.value = null
    loading.value = false
    
    // Clear Supabase session from localStorage to prevent auto-restore
    try {
      // Use Supabase's built-in method to clear session data
      await supabase.auth.signOut({ scope: 'local' })
    } catch (error) {
      console.warn('Could not clear Supabase session:', error)
    }
    
    // Always manually clear localStorage as additional safety measure
    try {
      // Clear all Supabase-related localStorage keys
      const keysToRemove = []
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i)
        if (key && (key.startsWith('sb-') || key.includes('supabase'))) {
          keysToRemove.push(key)
        }
      }
      
      keysToRemove.forEach(key => {
        localStorage.removeItem(key)
      })
      
      // Also try to clear the specific auth token key pattern
      const supabaseProjectId = supabaseUrl?.split('//')[1]?.split('.')[0]
      if (supabaseProjectId) {
        const authTokenKey = `sb-${supabaseProjectId}-auth-token`
        localStorage.removeItem(authTokenKey)
      }
    } catch (localError) {
      console.warn('Could not clear localStorage:', localError)
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

  function clearPasswordChangeFlag() {
    isPasswordChangeInProgress.value = false
  }

  return {
    user,
    session,
    loading,
    isInitialized,
    isAuthenticated,
    isPasswordChangeInProgress,
    signUp,
    registerWithBackend,
    signIn,
    signOut,
    clearSession,
    clearPasswordChangeFlag,
    initializeAuth,
    getAccessToken
  }
})
