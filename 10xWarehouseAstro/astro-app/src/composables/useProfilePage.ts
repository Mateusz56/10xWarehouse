import { ref, computed, watch } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { api } from '@/lib/api';
import { validateDisplayName, validatePassword, validatePasswordConfirmation, calculatePasswordStrength } from '@/lib/validation';
import type { 
  UserProfileDto, 
  DisplayNameFormData, 
  PasswordChangeFormData,
  ProfilePageState 
} from '@/types/dto';

export function useProfilePage() {
  const authStore = useAuthStore();
  
  // State
  const user = ref<UserProfileDto | null>(null);
  const accountCreatedAt = ref<string | null>(null);
  const displayNameLoading = ref(false);
  const passwordChangeLoading = ref(false);
  const displayNameError = ref<string | null>(null);
  const passwordChangeError = ref<string | null>(null);
  const displayNameSuccess = ref<string | null>(null);
  const passwordChangeSuccess = ref<string | null>(null);

  // Form data
  const displayNameForm = ref<DisplayNameFormData>({
    displayName: ''
  });

  const passwordChangeForm = ref<PasswordChangeFormData>({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });

  // Computed
  const displayNameValidation = computed(() => {
    return validateDisplayName(displayNameForm.value.displayName);
  });

  const isDisplayNameFormValid = computed(() => {
    return displayNameValidation.value.isValid;
  });

  const passwordValidation = computed(() => {
    return validatePassword(passwordChangeForm.value.newPassword);
  });

  const passwordConfirmationValidation = computed(() => {
    return validatePasswordConfirmation(
      passwordChangeForm.value.newPassword, 
      passwordChangeForm.value.confirmPassword
    );
  });

  const isPasswordChangeFormValid = computed(() => {
    return passwordChangeForm.value.currentPassword.length > 0 &&
           passwordValidation.value.isValid &&
           passwordConfirmationValidation.value.isValid;
  });

  const passwordStrength = computed(() => {
    return calculatePasswordStrength(passwordChangeForm.value.newPassword);
  });

  const hasDisplayNameChanges = computed(() => {
    return user.value && displayNameForm.value.displayName !== user.value.displayName;
  });

  // Initialize user data
  const initializeUser = () => {
    const currentUser = authStore.user;
    
    if (currentUser) {
      // Extract display name from user metadata
      const displayName = currentUser.user_metadata?.display_name || currentUser.email || 'User';
      
      user.value = {
        id: currentUser.id,
        email: currentUser.email || '',
        displayName: displayName
      };
      displayNameForm.value.displayName = displayName;
      // For now, we'll set a placeholder for account creation date
      accountCreatedAt.value = new Date().toISOString();
    }
  };

  // Watch for changes in auth store user
  watch(() => authStore.user, (newUser) => {
    if (newUser) {
      initializeUser();
    } else {
      user.value = null;
      accountCreatedAt.value = null;
    }
  }, { immediate: true });

  // Actions
  const updateDisplayName = async () => {
    if (!isDisplayNameFormValid.value || !hasDisplayNameChanges.value) return;
    
    displayNameLoading.value = true;
    displayNameError.value = null;
    displayNameSuccess.value = null;

    try {
      const updatedUser = await api.updateDisplayName({
        displayName: displayNameForm.value.displayName
      });
      
      user.value = updatedUser;
      
      // Update the auth store as well
      if (authStore.user) {
        authStore.user.user_metadata.display_name = updatedUser.displayName;
      }
      
      displayNameSuccess.value = 'Display name updated successfully';
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        displayNameSuccess.value = null;
      }, 3000);
    } catch (error: any) {
      // Enhanced error handling
      if (error.message?.includes('401') || error.message?.includes('Authentication')) {
        displayNameError.value = 'Your session has expired. Please log in again.';
        // Optionally redirect to login
        setTimeout(async () => {
          await authStore.clearSession();
          window.location.href = '/login';
        }, 2000);
      } else if (error.message?.includes('400') || error.message?.includes('validation')) {
        displayNameError.value = 'Please check your input and try again.';
      } else if (error.message?.includes('network') || error.message?.includes('fetch')) {
        displayNameError.value = 'Network error. Please check your connection and try again.';
      } else {
        displayNameError.value = error.message || 'Failed to update display name. Please try again.';
      }
    } finally {
      displayNameLoading.value = false;
    }
  };

  const changePassword = async () => {
    if (!isPasswordChangeFormValid.value) return;
    
    passwordChangeLoading.value = true;
    passwordChangeError.value = null;
    passwordChangeSuccess.value = null;

    try {
      await api.changePassword({
        currentPassword: passwordChangeForm.value.currentPassword,
        newPassword: passwordChangeForm.value.newPassword
      });
      
      passwordChangeSuccess.value = 'Password changed successfully. You will be logged out for security.';
      
      // Clear form
      resetPasswordChangeForm();
      
      // Wait 4 seconds to show success message, then clear session and redirect
      setTimeout(async () => {
        await authStore.clearSession();
        // Clear the flag before redirecting
        authStore.clearPasswordChangeFlag();
        //window.location.href = '/login';
      }, 4000);
    } catch (error: any) {
      // Enhanced error handling
      if (error.message?.includes('401') || error.message?.includes('Authentication')) {
        passwordChangeError.value = 'Your session has expired. Please log in again.';
        setTimeout(async () => {
          await authStore.clearSession();
          window.location.href = '/login';
        }, 2000);
      } else if (error.message?.includes('400') || error.message?.includes('validation')) {
        passwordChangeError.value = 'Please check your current password and try again.';
      } else if (error.message?.includes('403') || error.message?.includes('Forbidden')) {
        passwordChangeError.value = 'You do not have permission to change your password.';
      } else if (error.message?.includes('network') || error.message?.includes('fetch')) {
        passwordChangeError.value = 'Network error. Please check your connection and try again.';
      } else {
        passwordChangeError.value = error.message || 'Failed to change password. Please try again.';
      }
    } finally {
      passwordChangeLoading.value = false;
    }
  };

  const resetDisplayNameForm = () => {
    if (user.value) {
      displayNameForm.value.displayName = user.value.displayName;
    }
    displayNameError.value = null;
    displayNameSuccess.value = null;
  };

  const resetPasswordChangeForm = () => {
    passwordChangeForm.value = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    passwordChangeError.value = null;
    // Don't clear success message here - let it show until redirect
    // passwordChangeSuccess.value = null;
  };

  const clearMessages = () => {
    displayNameError.value = null;
    passwordChangeError.value = null;
    displayNameSuccess.value = null;
    passwordChangeSuccess.value = null;
  };

  // Profile page state
  const profilePageState = computed<ProfilePageState>(() => ({
    user: user.value,
    accountCreatedAt: accountCreatedAt.value,
    displayNameLoading: displayNameLoading.value,
    passwordChangeLoading: passwordChangeLoading.value,
    displayNameError: displayNameError.value,
    passwordChangeError: passwordChangeError.value,
    displayNameSuccess: displayNameSuccess.value,
    passwordChangeSuccess: passwordChangeSuccess.value
  }));

  return {
    // State
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
    profilePageState,
    
    // Computed
    isDisplayNameFormValid,
    isPasswordChangeFormValid,
    hasDisplayNameChanges,
    displayNameValidation,
    passwordValidation,
    passwordConfirmationValidation,
    passwordStrength,
    
    // Actions
    initializeUser,
    updateDisplayName,
    changePassword,
    resetDisplayNameForm,
    resetPasswordChangeForm,
    clearMessages
  };
}
