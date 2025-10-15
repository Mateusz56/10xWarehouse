<template>
  <td 
    class="p-4 align-middle [&:has([role=checkbox])]:pr-0"
    :class="className"
  >
    <slot />
  </td>
</template>

<script setup lang="ts">
interface Props {
  className?: string;
}

defineProps<Props>();
</script>
