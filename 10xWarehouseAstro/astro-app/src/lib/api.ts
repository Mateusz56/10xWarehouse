import type { CreateOrganizationRequestDto, OrganizationDto, UserDto } from "@/types/dto";
import { useAuthStore } from '@/stores/auth';

const API_BASE_URL = 'https://localhost:7146/api';

async function fetchWrapper<T>(url: string, options: RequestInit = {}): Promise<T> {
  const authStore = useAuthStore();
  const token = authStore.getAccessToken();
  
  const headers = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
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
