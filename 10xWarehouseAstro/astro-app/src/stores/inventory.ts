import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { 
  InventorySummaryDto,
  ProductSummaryDto,
  LocationSummaryDto,
  InventorySummaryState,
  InventoryFilters,
  PaginationDto,
  UserRole
} from '@/types/dto';
import { inventoryApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useInventoryStore = defineStore('inventory', () => {
  const organizationStore = useOrganizationStore();
  
  // State
  const inventory = ref<InventorySummaryDto[]>([]);
  const products = ref<ProductSummaryDto[]>([]);
  const locations = ref<LocationSummaryDto[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const pagination = ref<PaginationDto>({
    page: 1,
    pageSize: 50,
    total: 0
  });
  const currentPage = ref(1);
  const pageSize = ref(50);
  const filters = ref<InventoryFilters>({
    productId: undefined,
    locationId: undefined,
    lowStock: false
  });

  // Computed
  const hasInventory = computed(() => inventory.value.length > 0);
  const totalPages = computed(() => Math.ceil(pagination.value.total / pagination.value.pageSize));
  const isFirstPage = computed(() => currentPage.value === 1);
  const isLastPage = computed(() => currentPage.value === totalPages.value);
  const isEmpty = computed(() => !loading.value && inventory.value.length === 0);
  const filteredInventory = computed(() => {
    let filtered = inventory.value;
    
    if (filters.value.productId) {
      filtered = filtered.filter(item => item.product.id === filters.value.productId);
    }
    
    if (filters.value.locationId) {
      filtered = filtered.filter(item => item.location.id === filters.value.locationId);
    }
    
    if (filters.value.lowStock) {
      // Use the isLowStock flag from the backend
      filtered = filtered.filter(item => item.isLowStock);
    }
    
    return filtered;
  });
  const lowStockItems = computed(() => {
    return inventory.value.filter(item => item.isLowStock);
  });

  // Actions
  async function fetchMovements(): Promise<void> {
    // This method can be called to refresh stock movements
    // It will be implemented when needed
  }

  async function fetchInventory(page?: number, size?: number, newFilters?: InventoryFilters): Promise<void> {
    if (!organizationStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const targetPage = page ?? currentPage.value;
      const targetPageSize = size ?? pageSize.value;
      const targetFilters = newFilters ?? filters.value;
      
      // Validate pagination parameters
      if (targetPage < 1) {
        throw new Error('Page number must be greater than 0');
      }
      if (targetPageSize < 1 || targetPageSize > 100) {
        throw new Error('Page size must be between 1 and 100');
      }
      
      const response = await inventoryApi.getInventory(
        organizationStore.currentOrganizationId,
        targetPage,
        targetPageSize,
        targetFilters
      );

      inventory.value = response.data;

      // Ensure pagination object has all required properties
      pagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };
      
      currentPage.value = targetPage;
      pageSize.value = targetPageSize;
      filters.value = targetFilters;
    } catch (err) {
      console.error('Failed to fetch inventory:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load inventory';
      
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

  async function fetchProducts(): Promise<void> {
    if (!organizationStore.currentOrganizationId) {
      return;
    }

    try {
      const productsData = await inventoryApi.getProducts(organizationStore.currentOrganizationId, 50);
      products.value = productsData;
    } catch (err) {
      console.error('Failed to fetch products:', err);
    }
  }

  async function fetchLocations(): Promise<void> {
    if (!organizationStore.currentOrganizationId) {
      return;
    }

    try {
      const locationsData = await inventoryApi.getLocations(organizationStore.currentOrganizationId, 50);
      locations.value = locationsData;
    } catch (err) {
      console.error('Failed to fetch locations:', err);
    }
  }

  function setPage(page: number): void {
    currentPage.value = page;
  }

  function setPageSize(size: number): void {
    pageSize.value = size;
    currentPage.value = 1; // Reset to first page when changing page size
  }

  function setFilters(newFilters: InventoryFilters): void {
    filters.value = newFilters;
    currentPage.value = 1; // Reset to first page when changing filters
  }

  function clearFilters(): void {
    filters.value = {
      productId: undefined,
      locationId: undefined,
      lowStock: false
    };
    currentPage.value = 1;
  }

  function clearError(): void {
    error.value = null;
  }

  function clearInventory(): void {
    inventory.value = [];
    products.value = [];
    locations.value = [];
    pagination.value = {
      page: 1,
      pageSize: 50,
      total: 0
    };
    currentPage.value = 1;
    pageSize.value = 50;
    filters.value = {
      productId: undefined,
      locationId: undefined,
      lowStock: false
    };
    error.value = null;
  }

  // Role-based permissions
  const canPerformStockOperations = computed(() => {
    const role = organizationStore.currentRole as UserRole;
    return role === 'Owner' || role === 'Member';
  });

  return {
    // State
    inventory,
    products,
    locations,
    loading,
    error,
    pagination,
    currentPage,
    pageSize,
    filters,
    
    // Computed
    hasInventory,
    totalPages,
    isFirstPage,
    isLastPage,
    isEmpty,
    filteredInventory,
    lowStockItems,
    canPerformStockOperations,
    
    // Actions
    fetchMovements,
    fetchInventory,
    fetchProducts,
    fetchLocations,
    setPage,
    setPageSize,
    setFilters,
    clearFilters,
    clearError,
    clearInventory
  };
});
