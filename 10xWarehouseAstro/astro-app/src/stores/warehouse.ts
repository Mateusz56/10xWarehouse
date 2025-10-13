import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { 
  WarehouseDto, 
  WarehouseVM, 
  CreateWarehouseRequestDto, 
  UpdateWarehouseRequestDto,
  PaginationDto 
} from '@/types/dto';
import { warehouseApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useWarehouseStore = defineStore('warehouse', () => {
  const organizationStore = useOrganizationStore();
  
  // State
  const warehouses = ref<WarehouseVM[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const pagination = ref<PaginationDto>({
    page: 1,
    pageSize: 10,
    total: 0
  });
  const currentPage = ref(1);
  const pageSize = ref(10);

  // Computed
  const hasWarehouses = computed(() => warehouses.value.length > 0);
  const isEmpty = computed(() => !loading.value && warehouses.value.length === 0);
  const canCreateWarehouse = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner' || role === 'Member';
  });
  const canEditWarehouse = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner' || role === 'Member';
  });
  const canDeleteWarehouse = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner';
  });

  // Actions
  async function fetchWarehouses(page?: number, size?: number): Promise<void> {
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
      
      const response = await warehouseApi.getWarehouses(
        organizationStore.currentOrganizationId,
        targetPage,
        targetPageSize
      );

      warehouses.value = response.data.map(warehouse => ({
        id: warehouse.id,
        name: warehouse.name,
        organizationId: warehouse.organizationId,
        locationCount: 0 // Will be updated when we fetch warehouse details
      }));

      // Ensure pagination object has all required properties
      pagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };
      
      currentPage.value = targetPage;
      pageSize.value = targetPageSize;
    } catch (err) {
      console.error('Failed to fetch warehouses:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load warehouses';
      
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

  async function createWarehouse(data: CreateWarehouseRequestDto): Promise<WarehouseDto> {
    if (!organizationStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      const requestData = {
        ...data,
        organizationId: organizationStore.currentOrganizationId
      };

      const newWarehouse = await warehouseApi.createWarehouse(requestData);
      
      // Add to local state
      warehouses.value.unshift({
        id: newWarehouse.id,
        name: newWarehouse.name,
        organizationId: newWarehouse.organizationId,
        locationCount: 0
      });

      // Update pagination
      pagination.value.total += 1;

      return newWarehouse;
    } catch (err) {
      console.error('Failed to create warehouse:', err);
      error.value = err instanceof Error ? err.message : 'Failed to create warehouse';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateWarehouse(id: string, data: UpdateWarehouseRequestDto): Promise<WarehouseDto> {
    loading.value = true;
    error.value = null;

    try {
      const updatedWarehouse = await warehouseApi.updateWarehouse(id, data);
      
      // Update local state
      const index = warehouses.value.findIndex(w => w.id === id);
      if (index !== -1) {
        warehouses.value[index] = {
          ...warehouses.value[index],
          name: updatedWarehouse.name
        };
      }

      return updatedWarehouse;
    } catch (err) {
      console.error('Failed to update warehouse:', err);
      error.value = err instanceof Error ? err.message : 'Failed to update warehouse';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteWarehouse(id: string): Promise<void> {
    loading.value = true;
    error.value = null;

    try {
      await warehouseApi.deleteWarehouse(id);
      
      // Remove from local state
      warehouses.value = warehouses.value.filter(w => w.id !== id);
      
      // Update pagination
      pagination.value.total -= 1;

      // If current page is empty and not the first page, go to previous page
      if (warehouses.value.length === 0 && currentPage.value > 1) {
        await fetchWarehouses(currentPage.value - 1, pageSize.value);
      }
    } catch (err) {
      console.error('Failed to delete warehouse:', err);
      error.value = err instanceof Error ? err.message : 'Failed to delete warehouse';
      throw err;
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

  function clearWarehouses(): void {
    warehouses.value = [];
    pagination.value = {
      page: 1,
      pageSize: 10,
      total: 0
    };
    currentPage.value = 1;
    pageSize.value = 10;
    error.value = null;
  }

  return {
    // State
    warehouses,
    loading,
    error,
    pagination,
    currentPage,
    pageSize,
    
    // Computed
    hasWarehouses,
    isEmpty,
    canCreateWarehouse,
    canEditWarehouse,
    canDeleteWarehouse,
    
    // Actions
    fetchWarehouses,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
    setPage,
    setPageSize,
    clearError,
    clearWarehouses
  };
});
