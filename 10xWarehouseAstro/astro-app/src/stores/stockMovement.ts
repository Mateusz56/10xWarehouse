import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { 
  StockMovementDto, 
  StockMovementLogState,
  PaginationDto 
} from '@/types/dto';
import { stockMovementApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useStockMovementStore = defineStore('stockMovement', () => {
  const organizationStore = useOrganizationStore();
  
  // State
  const movements = ref<StockMovementDto[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const pagination = ref<PaginationDto>({
    page: 1,
    pageSize: 50,
    total: 0
  });
  const currentPage = ref(1);
  const pageSize = ref(50); // Default page size is 50

  // Computed
  const hasMovements = computed(() => movements.value.length > 0);
  const totalPages = computed(() => Math.ceil(pagination.value.total / pagination.value.pageSize));
  const isFirstPage = computed(() => currentPage.value === 1);
  const isLastPage = computed(() => currentPage.value === totalPages.value);
  const isEmpty = computed(() => !loading.value && movements.value.length === 0);

  // Actions
  async function fetchMovements(page?: number, size?: number, productTemplateId?: string, locationId?: string): Promise<void> {
    if (!organizationStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const targetPage = page ?? currentPage.value;
      const targetPageSize = size ?? pageSize.value;
      
      // Validate pagination parameters
      if (targetPage < 1) {
        throw new Error('Page number must be greater than 0');
      }
      if (targetPageSize < 1 || targetPageSize > 100) {
        throw new Error('Page size must be between 1 and 100');
      }
      
      const response = await stockMovementApi.getStockMovements(
        organizationStore.currentOrganizationId,
        targetPage,
        targetPageSize,
        productTemplateId,
        locationId
      );

      movements.value = response.data;

      // Ensure pagination object has all required properties
      pagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };
      
      currentPage.value = targetPage;
      pageSize.value = targetPageSize;
    } catch (err) {
      console.error('Failed to fetch stock movements:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load stock movements';
      
      // If auth error, sign out user
      if (err instanceof Error && err.message === 'Authentication required') {
        const { useAuthStore } = await import('@/stores/auth');
        const authStore = useAuthStore();
        await authStore.signOut();
      }
    } finally {
      loading.value = false;
    }
  }

  function setPage(page: number): void {
    currentPage.value = page;
  }

  function setPageSize(size: number): void {
    pageSize.value = size;
    currentPage.value = 1; // Reset to first page when changing page size
  }

  function clearError(): void {
    error.value = null;
  }

  function clearMovements(): void {
    movements.value = [];
    pagination.value = {
      page: 1,
      pageSize: 50,
      total: 0
    };
    currentPage.value = 1;
    pageSize.value = 50;
    error.value = null;
  }

  return {
    // State
    movements,
    loading,
    error,
    pagination,
    currentPage,
    pageSize,
    
    // Computed
    hasMovements,
    totalPages,
    isFirstPage,
    isLastPage,
    isEmpty,
    
    // Actions
    fetchMovements,
    setPage,
    setPageSize,
    clearError,
    clearMovements
  };
});
