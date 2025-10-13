import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { 
  WarehouseWithLocationsDto,
  LocationDto,
  LocationVM,
  CreateLocationRequestDto, 
  UpdateLocationRequestDto,
  UpdateWarehouseRequestDto,
  WarehouseDto,
  PaginationDto 
} from '@/types/dto';
import { warehouseApi, locationApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useWarehouseDetailsStore = defineStore('warehouseDetails', () => {
  const organizationStore = useOrganizationStore();
  
  // State
  const warehouse = ref<WarehouseWithLocationsDto | null>(null);
  const locations = ref<LocationVM[]>([]);
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
  const hasLocations = computed(() => locations.value.length > 0);
  const isEmpty = computed(() => !loading.value && locations.value.length === 0);
  const canCreateLocation = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner' || role === 'Member';
  });
  const canEditLocation = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner' || role === 'Member';
  });
  const canDeleteLocation = computed(() => {
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
  async function fetchWarehouseDetails(warehouseId: string): Promise<void> {
    if (!warehouseId) {
      error.value = 'Warehouse ID is required';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const warehouseData = await warehouseApi.getWarehouse(warehouseId);
      warehouse.value = warehouseData;
      
      // Initialize locations from warehouse data
      locations.value = warehouseData.locations.map(location => ({
        id: location.id,
        name: location.name,
        description: location.description,
        warehouseId: location.warehouseId
      }));
    } catch (err) {
      console.error('Failed to fetch warehouse details:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load warehouse details';
      
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

  async function fetchLocations(warehouseId: string, page?: number, size?: number): Promise<void> {
    if (!warehouseId) {
      error.value = 'Warehouse ID is required';
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
      
      const response = await locationApi.getLocations(
        warehouseId,
        targetPage,
        targetPageSize
      );

      locations.value = response.data.map(location => ({
        id: location.id,
        name: location.name,
        description: location.description,
        warehouseId: location.warehouseId
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
      console.error('Failed to fetch locations:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load locations';
      
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

  async function createLocation(data: CreateLocationRequestDto): Promise<LocationDto> {
    loading.value = true;
    error.value = null;

    try {
      const newLocation = await locationApi.createLocation(data);
      
      // Add to local state
      locations.value.unshift({
        id: newLocation.id,
        name: newLocation.name,
        description: newLocation.description,
        warehouseId: newLocation.warehouseId
      });

      // Update pagination
      pagination.value.total += 1;

      return newLocation;
    } catch (err) {
      console.error('Failed to create location:', err);
      error.value = err instanceof Error ? err.message : 'Failed to create location';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateLocation(id: string, data: UpdateLocationRequestDto): Promise<LocationDto> {
    loading.value = true;
    error.value = null;

    try {
      const updatedLocation = await locationApi.updateLocation(id, data);
      
      // Update local state
      const index = locations.value.findIndex(l => l.id === id);
      if (index !== -1) {
        locations.value[index] = {
          ...locations.value[index],
          name: updatedLocation.name,
          description: updatedLocation.description
        };
      }

      return updatedLocation;
    } catch (err) {
      console.error('Failed to update location:', err);
      error.value = err instanceof Error ? err.message : 'Failed to update location';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteLocation(id: string): Promise<void> {
    loading.value = true;
    error.value = null;

    try {
      await locationApi.deleteLocation(id);
      
      // Remove from local state
      locations.value = locations.value.filter(l => l.id !== id);
      
      // Update pagination
      pagination.value.total -= 1;

      // If current page is empty and not the first page, go to previous page
      if (locations.value.length === 0 && currentPage.value > 1) {
        const warehouseId = warehouse.value?.id;
        if (warehouseId) {
          await fetchLocations(warehouseId, currentPage.value - 1, pageSize.value);
        }
      }
    } catch (err) {
      console.error('Failed to delete location:', err);
      error.value = err instanceof Error ? err.message : 'Failed to delete location';
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
      if (warehouse.value && warehouse.value.id === id) {
        warehouse.value = {
          ...warehouse.value,
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
      
      // Clear local state
      warehouse.value = null;
      locations.value = [];
      pagination.value = {
        page: 1,
        pageSize: 10,
        total: 0
      };
      currentPage.value = 1;
      pageSize.value = 10;
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

  function clearWarehouseDetails(): void {
    warehouse.value = null;
    locations.value = [];
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
    warehouse,
    locations,
    loading,
    error,
    pagination,
    currentPage,
    pageSize,
    
    // Computed
    hasLocations,
    isEmpty,
    canCreateLocation,
    canEditLocation,
    canDeleteLocation,
    canEditWarehouse,
    canDeleteWarehouse,
    
    // Actions
    fetchWarehouseDetails,
    fetchLocations,
    createLocation,
    updateLocation,
    deleteLocation,
    updateWarehouse,
    deleteWarehouse,
    setPage,
    setPageSize,
    clearError,
    clearWarehouseDetails
  };
});
