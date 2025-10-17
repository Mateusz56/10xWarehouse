<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  class?: string;
}>();

const contentClasses = computed(() => [
  'p-6 pt-0',
  props.class
]);
</script>

<template>
  <div :class="contentClasses">
    <slot />
  </div>
</template>
