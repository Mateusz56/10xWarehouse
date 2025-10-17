<script setup lang="ts">
import { computed } from 'vue';
import type { PasswordStrength } from '@/types/dto';

const props = defineProps<{
  password: string;
  strength: PasswordStrength;
}>();

const strengthColor = computed(() => {
  const colorMap = {
    red: 'bg-red-500',
    orange: 'bg-orange-500',
    yellow: 'bg-yellow-500',
    blue: 'bg-blue-500',
    green: 'bg-green-500'
  };
  return colorMap[props.strength.color];
});

const strengthTextColor = computed(() => {
  const colorMap = {
    red: 'text-red-600',
    orange: 'text-orange-600',
    yellow: 'text-yellow-600',
    blue: 'text-blue-600',
    green: 'text-green-600'
  };
  return colorMap[props.strength.color];
});

const progressWidth = computed(() => {
  return `${(props.strength.score / 4) * 100}%`;
});
</script>

<template>
  <div class="space-y-2">
    <div class="flex items-center space-x-2">
      <div class="flex-1 bg-gray-200 rounded-full h-2">
        <div 
          :class="strengthColor"
          class="h-2 rounded-full transition-all duration-300"
          :style="{ width: progressWidth }"
        ></div>
      </div>
      <span :class="strengthTextColor" class="text-sm font-medium">
        {{ strength.label }}
      </span>
    </div>
  </div>
</template>
