<script setup lang="ts">
import { computed } from 'vue';
import { Card, CardHeader, CardContent, CardFooter } from '@/components/ui/card';
import type { UserProfileDto } from '@/types/dto';

const props = defineProps<{
  user: UserProfileDto | null;
  accountCreatedAt: string;
}>();

const formattedDate = computed(() => {
  try {
    return new Date(props.accountCreatedAt).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  } catch {
    return 'Unknown';
  }
});
</script>

<template>
  <Card>
    <CardHeader>
      <h2 class="text-xl font-semibold text-foreground">Account Information</h2>
      <p class="text-sm text-muted-foreground">Your account details and information</p>
    </CardHeader>
    
    <CardContent>
      <div v-if="!user" class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-1">
            <label class="text-sm font-medium text-muted-foreground">Email Address</label>
            <div class="h-4 bg-gray-100 rounded animate-pulse"></div>
          </div>
          
          <div class="space-y-1">
            <label class="text-sm font-medium text-muted-foreground">Account Created</label>
            <div class="h-4 bg-gray-100 rounded animate-pulse"></div>
          </div>
        </div>
        <p class="text-sm text-gray-500">Loading account information...</p>
      </div>
      <div v-else class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-1">
            <label class="text-sm font-medium text-muted-foreground">Email Address</label>
            <p class="text-sm text-foreground">{{ user.email }}</p>
          </div>
          
          <div class="space-y-1">
            <label class="text-sm font-medium text-muted-foreground">Account Created</label>
            <p class="text-sm text-foreground">{{ formattedDate }}</p>
          </div>
        </div>
      </div>
    </CardContent>
    
    <CardFooter>
      <div class="text-sm text-muted-foreground">
        For security reasons, some account information cannot be changed from this page.
      </div>
    </CardFooter>
  </Card>
</template>
