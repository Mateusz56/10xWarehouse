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
  PaginatedResponseDto
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
