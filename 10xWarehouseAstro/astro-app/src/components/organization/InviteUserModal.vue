<template>
  <Dialog :open="isOpen" @update:open="handleClose">
    <DialogContent class="sm:max-w-[500px]">
      <DialogHeader>
        <DialogTitle>Invite User to Organization</DialogTitle>
        <DialogDescription>
          Search for a user and invite them to join your organization with a specific role.
        </DialogDescription>
      </DialogHeader>
      
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <!-- User Search -->
        <div class="space-y-2">
          <Label for="user-search">Search User</Label>
          <div class="relative">
            <Input
              id="user-search"
              v-model="searchQuery"
              type="text"
              placeholder="Search by email or name"
              :class="{ 'border-destructive': userError }"
              @input="handleSearchInput"
              @focus="showSearchResults = true"
              @blur="handleSearchBlur"
              required
            />
            <!-- Clear Button -->
            <button
              v-if="selectedUser"
              type="button"
              @click="clearUserSelection"
              class="absolute inset-y-0 right-0 pr-3 flex items-center text-muted-foreground hover:text-foreground"
            >
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
            <!-- Search Results Dropdown -->
            <div
              v-if="showSearchResults && (searchResults.length > 0 || isSearching)"
              class="absolute z-10 mt-1 w-full bg-card border border-border rounded-md shadow-lg max-h-60 overflow-auto"
            >
              <!-- Loading State -->
              <div v-if="isSearching" class="px-4 py-3 flex items-center justify-center">
                <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-primary mr-2"></div>
                <span class="text-sm text-muted-foreground">Searching...</span>
              </div>
              <!-- Search Results -->
              <div
                v-for="user in searchResults"
                :key="user.id"
                @click="selectUser(user)"
                class="px-4 py-3 hover:bg-muted cursor-pointer border-b border-border last:border-b-0"
              >
                <div class="flex items-center space-x-3">
                  <div class="h-8 w-8 rounded-full bg-muted flex items-center justify-center">
                    <svg class="h-4 w-4 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-card-foreground">{{ user.displayName }}</p>
                    <p class="text-sm text-muted-foreground">{{ user.email }}</p>
                  </div>
                </div>
              </div>
              <!-- No Results -->
              <div
                v-if="!isSearching && searchResults.length === 0 && searchQuery.length > 2"
                class="px-4 py-3"
              >
                <p class="text-sm text-muted-foreground text-center">No users found</p>
              </div>
            </div>
          </div>
          <p v-if="userError" class="text-sm text-destructive">{{ userError }}</p>
        </div>

        <!-- Role Selection -->
        <div class="space-y-2">
          <Label for="user-role">Role</Label>
          <Select v-model="selectedRole" required>
            <SelectTrigger :class="{ 'border-destructive': roleError }">
              <SelectValue placeholder="Select a role" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Member">Member</SelectItem>
              <SelectItem value="Viewer">Viewer</SelectItem>
            </SelectContent>
          </Select>
          <p v-if="roleError" class="text-sm text-destructive">{{ roleError }}</p>
        </div>

        <!-- Error Display -->
        <div v-if="error" class="rounded-md bg-destructive/10 p-3">
          <p class="text-sm text-destructive">{{ error }}</p>
        </div>

        <DialogFooter>
          <Button type="button" variant="outline" @click="handleClose" :disabled="loading">
            Cancel
          </Button>
          <Button type="submit" :disabled="loading || !selectedUser || !selectedRole">
            {{ loading ? 'Sending Invitation...' : 'Send Invitation' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed, onUnmounted } from 'vue';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { organizationApi } from '@/lib/api';
import type { InviteUserModalProps, CreateInvitationRequestDto, UserSearchResult } from '@/types/dto';

const props = defineProps<InviteUserModalProps>();

const emit = defineEmits<{
  close: [];
  submit: [data: CreateInvitationRequestDto];
}>();

const searchQuery = ref('');
const selectedUser = ref<UserSearchResult | null>(null);
const selectedRole = ref<'Member' | 'Viewer' | ''>('');
const userError = ref('');
const roleError = ref('');
const searchResults = ref<UserSearchResult[]>([]);
const showSearchResults = ref(false);
const isSearching = ref(false);

// No mock data needed - using real API

// Reset form when modal opens
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    searchQuery.value = '';
    selectedUser.value = null;
    selectedRole.value = '';
    userError.value = '';
    roleError.value = '';
    searchResults.value = [];
    showSearchResults.value = false;
  }
});

const handleClose = () => {
  if (!props.loading) {
    emit('close');
  }
};

let searchTimeout: NodeJS.Timeout | null = null;

const handleSearchInput = async () => {
  if (searchQuery.value.length < 2) {
    searchResults.value = [];
    showSearchResults.value = false;
    return;
  }

  // Clear previous timeout
  if (searchTimeout) {
    clearTimeout(searchTimeout);
  }

  // Debounce search by 300ms
  searchTimeout = setTimeout(async () => {
    isSearching.value = true;
    showSearchResults.value = true;

    try {
      // Call the real API
      const users = await organizationApi.searchUsers(searchQuery.value, 10);
      searchResults.value = users;
    } catch (error) {
      console.error('Error searching users:', error);
      searchResults.value = [];
    } finally {
      isSearching.value = false;
    }
  }, 300);
};

const handleSearchBlur = () => {
  // Delay hiding search results to allow for clicks
  setTimeout(() => {
    showSearchResults.value = false;
  }, 200);
};

const selectUser = (user: UserSearchResult) => {
  selectedUser.value = user;
  searchQuery.value = `${user.displayName} (${user.email})`;
  showSearchResults.value = false;
  userError.value = '';
};

const clearUserSelection = () => {
  selectedUser.value = null;
  searchQuery.value = '';
  searchResults.value = [];
  showSearchResults.value = false;
  userError.value = '';
};

const handleSubmit = () => {
  // Reset errors
  userError.value = '';
  roleError.value = '';

  // Validate form
  if (!selectedUser.value) {
    userError.value = 'Please select a user';
    return;
  }

  if (!selectedRole.value) {
    roleError.value = 'Role is required';
    return;
  }

  // Submit form
  emit('submit', {
    invitedUserId: selectedUser.value.id,
    role: selectedRole.value as 'Member' | 'Viewer'
  });
};

// Cleanup timeout on unmount
onUnmounted(() => {
  if (searchTimeout) {
    clearTimeout(searchTimeout);
  }
});
</script>