<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  class?: string;
}>();

const headerClasses = computed(() => [
  'flex flex-col space-y-1.5 p-6',
  props.class
]);
</script>

<template>
  <div :class="headerClasses">
    <slot />
  </div>
</template>
