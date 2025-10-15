<template>
  <div class="flex flex-col items-center justify-center p-8 text-center">
    <div class="mb-4 text-4xl text-muted-foreground">
      <slot name="icon">
        <svg
          class="h-12 w-12"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
          />
        </svg>
      </slot>
    </div>
    
    <h3 class="mb-2 text-lg font-semibold">{{ title }}</h3>
    
    <p class="mb-4 text-sm text-muted-foreground max-w-sm">
      {{ message }}
    </p>
    
    <Button v-if="actionText && onAction" @click="onAction" :disabled="actionDisabled">
      {{ actionText }}
    </Button>
  </div>
</template>

<script setup lang="ts">
import { Button } from './button';

interface Props {
  title?: string;
  message: string;
  actionText?: string;
  actionDisabled?: boolean;
  onAction?: () => void;
}

defineProps<Props>();
</script>
