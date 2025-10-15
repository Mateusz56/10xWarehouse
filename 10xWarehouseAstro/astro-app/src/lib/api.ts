import type { 
  CreateOrganizationRequestDto, 
  OrganizationDto, 
  UserDto,
  WarehouseDto,
  WarehouseWithLocationsDto,
  CreateWarehouseRequestDto,
  UpdateWarehouseRequestDto,
  LocationDto,
  CreateLocationRequestDto,
  UpdateLocationRequestDto,
  PaginatedResponseDto,
  ProductTemplateDto,
  CreateProductTemplateRequestDto,
  StockMovementDto,
  CreateStockMovementCommand,
  InventorySummaryDto,
  ProductSummaryDto,
  LocationSummaryDto,
  InventoryFilters,
  StockOperationType
} from "@/types/dto";
import { useAuthStore } from '@/stores/auth';

const API_BASE_URL = 'https://localhost:7146/api';

async function fetchWrapper<T>(url: string, options: RequestInit = {}): Promise<T> {
  const authStore = useAuthStore();
  const token = authStore.getAccessToken();
  
  // Get current organization ID from the organization store
  const { useOrganizationStore } = await import('@/stores/organization');
  const organizationStore = useOrganizationStore();
  const currentOrgId = organizationStore.currentOrganizationId;
  
  const headers = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
    ...(currentOrgId && { 'X-Organization-Id': currentOrgId }),
    ...options.headers,
  };

  const response = await fetch(url, { ...options, headers });

  if (!response.ok) {
    if (response.status === 401) {
      // Token expired or invalid, sign out user
      await authStore.signOut();
      throw new Error('Authentication required');
    }
    
    const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
    throw new Error(errorData.message || 'Failed to fetch');
  }

  // Check if response has content and is JSON
  const contentType = response.headers.get('content-type');
  if (!contentType || !contentType.includes('application/json')) {
    // For responses without JSON content (like 204 No Content), return undefined
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export const api = {
  async getUserData(): Promise<UserDto> {
    return fetchWrapper<UserDto>(`${API_BASE_URL}/users/me`);
  },

  async createOrganization(data: CreateOrganizationRequestDto): Promise<OrganizationDto> {
    return fetchWrapper<OrganizationDto>(`${API_BASE_URL}/organizations`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },
};

export const warehouseApi = {
  async getWarehouses(organizationId: string, page: number = 1, pageSize: number = 10): Promise<PaginatedResponseDto<WarehouseDto>> {
    return fetchWrapper<PaginatedResponseDto<WarehouseDto>>(
      `${API_BASE_URL}/warehouses?organizationId=${organizationId}&page=${page}&pageSize=${pageSize}`
    );
  },

  async getWarehouse(id: string): Promise<WarehouseWithLocationsDto> {
    return fetchWrapper<WarehouseWithLocationsDto>(`${API_BASE_URL}/warehouses/${id}`);
  },

  async createWarehouse(data: CreateWarehouseRequestDto): Promise<WarehouseDto> {
    return fetchWrapper<WarehouseDto>(`${API_BASE_URL}/warehouses`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async updateWarehouse(id: string, data: UpdateWarehouseRequestDto): Promise<WarehouseDto> {
    return fetchWrapper<WarehouseDto>(`${API_BASE_URL}/warehouses/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  async deleteWarehouse(id: string): Promise<void> {
    return fetchWrapper<void>(`${API_BASE_URL}/warehouses/${id}`, {
      method: 'DELETE',
    });
  }
};

export const locationApi = {
  async getLocations(warehouseId: string, page: number = 1, pageSize: number = 10): Promise<PaginatedResponseDto<LocationDto>> {
    return fetchWrapper<PaginatedResponseDto<LocationDto>>(
      `${API_BASE_URL}/locations?warehouseId=${warehouseId}&page=${page}&pageSize=${pageSize}`
    );
  },

  async getLocation(id: string): Promise<LocationDto> {
    return fetchWrapper<LocationDto>(`${API_BASE_URL}/locations/${id}`);
  },

  async createLocation(data: CreateLocationRequestDto): Promise<LocationDto> {
    return fetchWrapper<LocationDto>(`${API_BASE_URL}/locations`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async updateLocation(id: string, data: UpdateLocationRequestDto): Promise<LocationDto> {
    return fetchWrapper<LocationDto>(`${API_BASE_URL}/locations/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  async deleteLocation(id: string): Promise<void> {
    return fetchWrapper<void>(`${API_BASE_URL}/locations/${id}`, {
      method: 'DELETE',
    });
  }
};

export const productTemplateApi = {
  async getProductTemplates(organizationId: string, page: number = 1, pageSize: number = 10, search?: string): Promise<PaginatedResponseDto<ProductTemplateDto>> {
    const searchParam = search ? `&search=${encodeURIComponent(search)}` : '';
    return fetchWrapper<PaginatedResponseDto<ProductTemplateDto>>(
      `${API_BASE_URL}/producttemplates?organizationId=${organizationId}&page=${page}&pageSize=${pageSize}${searchParam}`
    );
  },

  async getProductTemplate(id: string): Promise<ProductTemplateDto> {
    return fetchWrapper<ProductTemplateDto>(`${API_BASE_URL}/producttemplates/${id}`);
  },

  async createProductTemplate(data: CreateProductTemplateRequestDto): Promise<ProductTemplateDto> {
    return fetchWrapper<ProductTemplateDto>(`${API_BASE_URL}/producttemplates`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async updateProductTemplate(id: string, data: CreateProductTemplateRequestDto): Promise<ProductTemplateDto> {
    return fetchWrapper<ProductTemplateDto>(`${API_BASE_URL}/producttemplates/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  async deleteProductTemplate(id: string): Promise<void> {
    return fetchWrapper<void>(`${API_BASE_URL}/producttemplates/${id}`, {
      method: 'DELETE',
    });
  }
};

export const stockMovementApi = {
  async getStockMovements(
    organizationId: string, 
    page: number = 1, 
    pageSize: number = 50,
    productTemplateId?: string,
    locationId?: string
  ): Promise<PaginatedResponseDto<StockMovementDto>> {
    const params = new URLSearchParams({
      organizationId,
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (productTemplateId) params.append('productTemplateId', productTemplateId);
    if (locationId) params.append('locationId', locationId);
    
    return fetchWrapper<PaginatedResponseDto<StockMovementDto>>(
      `${API_BASE_URL}/stockmovements?${params.toString()}`
    );
  },

  async createStockMovement(
    organizationId: string,
    command: CreateStockMovementCommand
  ): Promise<StockMovementDto> {
    return fetchWrapper<StockMovementDto>(`${API_BASE_URL}/stockmovements?organizationId=${organizationId}`, {
      method: 'POST',
      body: JSON.stringify(command),
    });
  }
};

export const inventoryApi = {
  async getInventory(
    organizationId: string,
    page: number = 1,
    pageSize: number = 50,
    filters: InventoryFilters = { lowStock: false }
  ): Promise<PaginatedResponseDto<InventorySummaryDto>> {
    const params = new URLSearchParams({
      organizationId,
      page: page.toString(),
      pageSize: pageSize.toString(),
      lowStock: filters.lowStock.toString(),
    });
    
    if (filters.productId) params.append('productTemplateId', filters.productId);
    if (filters.locationId) params.append('locationId', filters.locationId);
    
    return fetchWrapper<PaginatedResponseDto<InventorySummaryDto>>(
      `${API_BASE_URL}/inventory?${params.toString()}`
    );
  },

  async getProducts(organizationId: string, pageSize: number = 50): Promise<ProductSummaryDto[]> {
    const response = await fetchWrapper<PaginatedResponseDto<ProductTemplateDto>>(
      `${API_BASE_URL}/producttemplates?organizationId=${organizationId}&page=1&pageSize=${pageSize}`
    );
    return response.data.map(product => ({
      id: product.id,
      name: product.name
    }));
  },

  async getLocations(organizationId: string, pageSize: number = 50): Promise<LocationSummaryDto[]> {
    // Get all warehouses first, then get locations from each warehouse
    const warehousesResponse = await fetchWrapper<PaginatedResponseDto<WarehouseDto>>(
      `${API_BASE_URL}/warehouses?organizationId=${organizationId}&page=1&pageSize=${pageSize}`
    );
    
    const allLocations: LocationSummaryDto[] = [];
    
    for (const warehouse of warehousesResponse.data) {
      const locationsResponse = await fetchWrapper<PaginatedResponseDto<LocationDto>>(
        `${API_BASE_URL}/locations?warehouseId=${warehouse.id}&page=1&pageSize=${pageSize}`
      );
      
      allLocations.push(...locationsResponse.data.map(location => ({
        id: location.id,
        name: location.name
      })));
    }
    
    return allLocations;
  }
};
