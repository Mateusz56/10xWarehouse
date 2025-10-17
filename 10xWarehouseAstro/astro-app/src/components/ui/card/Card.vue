<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  class?: string;
}>();

const cardClasses = computed(() => [
  'rounded-lg border bg-card text-card-foreground shadow-sm',
  props.class
]);
</script>

<template>
  <div :class="cardClasses">
    <slot />
  </div>
</template>
