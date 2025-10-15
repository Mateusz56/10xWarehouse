<template>
  <TableRow>
    <TableCell>{{ formattedDate }}</TableCell>
    <TableCell>{{ product.name }}</TableCell>
    <TableCell>
      <span 
        class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
        :class="movementTypeClass"
      >
        {{ movementTypeLabel }}
      </span>
    </TableCell>
    <TableCell class="text-right">
      <span :class="deltaClass">
        {{ deltaText }}
      </span>
    </TableCell>
    <TableCell>
      <div class="flex flex-col space-y-1">
        <span v-if="fromLocation" class="text-sm text-muted-foreground">
          From: {{ fromLocation.name }}
        </span>
        <span v-if="toLocation" class="text-sm">
          To: {{ toLocation.name }}
        </span>
        <span v-if="!fromLocation && !toLocation" class="text-sm text-muted-foreground">
          {{ location?.name || 'Unknown' }}
        </span>
      </div>
    </TableCell>
    <TableCell class="text-right">{{ quantityBefore }}</TableCell>
    <TableCell class="text-right">{{ quantityAfter }}</TableCell>
  </TableRow>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { TableRow, TableCell } from './ui/table';
import type { 
  StockMovementDto, 
  ProductSummaryDto, 
  LocationSummaryDto,
  MovementTypeDisplay 
} from '@/types/dto';

interface Props {
  movement: StockMovementDto;
  product: ProductSummaryDto;
  fromLocation?: LocationSummaryDto;
  toLocation?: LocationSummaryDto;
  location?: LocationSummaryDto;
  quantityBefore?: number;
  quantityAfter?: number;
}

const props = defineProps<Props>();

const movementTypeConfig: Record<string, MovementTypeDisplay> = {
  Add: { label: 'Add', color: 'bg-green-100 text-green-800', icon: '➕' },
  Withdraw: { label: 'Withdraw', color: 'bg-red-100 text-red-800', icon: '➖' },
  Move: { label: 'Move', color: 'bg-blue-100 text-blue-800', icon: '↔️' },
  Reconcile: { label: 'Reconcile', color: 'bg-yellow-100 text-yellow-800', icon: '🔄' }
};

const formattedDate = computed(() => {
  return new Date(props.movement.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
});

const movementTypeLabel = computed(() => {
  return movementTypeConfig[props.movement.movementType]?.label || props.movement.movementType;
});

const movementTypeClass = computed(() => {
  return movementTypeConfig[props.movement.movementType]?.color || 'bg-gray-100 text-gray-800';
});

const deltaText = computed(() => {
  const delta = props.movement.delta;
  return delta > 0 ? `+${delta}` : delta.toString();
});

const deltaClass = computed(() => {
  return props.movement.delta > 0 
    ? 'text-green-600 font-medium' 
    : props.movement.delta < 0 
    ? 'text-red-600 font-medium' 
    : 'text-muted-foreground';
});
</script>
