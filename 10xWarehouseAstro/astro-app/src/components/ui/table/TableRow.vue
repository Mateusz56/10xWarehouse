<template>
  <tr 
    class="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted"
    :class="className"
  >
    <slot />
  </tr>
</template>

<script setup lang="ts">
interface Props {
  className?: string;
}

defineProps<Props>();
</script>
