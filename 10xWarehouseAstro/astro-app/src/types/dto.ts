export interface OrganizationDto {
  id: string;
  name: string;
}

export interface CreateOrganizationRequestDto {
  name: string;
}

export interface MembershipDto {
  organizationId: string;
  organizationName: string;
  role: 'Owner' | 'Member' | 'Viewer';
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  memberships: MembershipDto[];
}

export interface OrganizationVM {
  id: string;
  name: string;
}

export interface UserVM {
  id: string;
  email: string;
  displayName: string;
  currentOrganizationId: string;
  currentRole: 'Owner' | 'Member' | 'Viewer';
  organizations: OrganizationVM[];
  memberships: MembershipDto[];
}

export interface NavItemVM {
  to: string;
  label: string;
  icon: string;
  requiredRole?: 'Owner';
}

// Warehouse DTOs
export interface WarehouseDto {
  id: string;
  name: string;
  organizationId: string;
}

export interface WarehouseWithLocationsDto {
  id: string;
  name: string;
  organizationId: string;
  locations: LocationDto[];
}

export interface LocationDto {
  id: string;
  name: string;
  description?: string;
  warehouseId: string;
}

// Warehouse Request DTOs
export interface CreateWarehouseRequestDto {
  name: string;
  organizationId: string;
}

export interface UpdateWarehouseRequestDto {
  name: string;
}

// Location Request DTOs
export interface CreateLocationRequestDto {
  name: string;
  description?: string;
  warehouseId: string;
}

export interface UpdateLocationRequestDto {
  name: string;
  description?: string;
}

// Pagination DTOs (matching backend structure)
export interface PaginationDto {
  page: number;
  pageSize: number;
  total: number;
}

export interface PaginatedResponseDto<T> {
  data: T[];
  pagination: PaginationDto;
}

export interface PaginationRequestDto {
  page: number;
  pageSize: number;
}

// Warehouse View Models
export interface WarehouseVM {
  id: string;
  name: string;
  organizationId: string;
  locationCount?: number;
}

export interface WarehousePageState {
  warehouses: WarehouseVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}

// Location View Models
export interface LocationVM {
  id: string;
  name: string;
  description?: string;
  warehouseId: string;
}

// Warehouse Details View Models
export interface WarehouseDetailsState {
  warehouse: WarehouseWithLocationsDto | null;
  locations: LocationVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}

export interface WarehouseDetailsVM {
  warehouse: WarehouseWithLocationsDto;
  locations: LocationVM[];
  pagination: PaginationDto;
}

// Product Template DTOs
export interface ProductTemplateDto {
  id: string;
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}

export interface CreateProductTemplateRequestDto {
  organizationId: string;
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}

// Product Template View Models
export interface ProductTemplateVM {
  id: string;
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}

export interface ProductPageState {
  products: ProductTemplateVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  searchQuery: string;
}