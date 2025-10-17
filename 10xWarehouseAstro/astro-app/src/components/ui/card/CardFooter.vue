<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  class?: string;
}>();

const footerClasses = computed(() => [
  'flex items-center p-6 pt-0',
  props.class
]);
</script>

<template>
  <div :class="footerClasses">
    <slot />
  </div>
</template>
