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

// Stock Movement DTOs
export interface StockMovementDto {
  id: string;
  productTemplateId: string;
  movementType: 'Add' | 'Withdraw' | 'Move' | 'Reconcile';
  delta: number;
  fromLocationId?: string;
  toLocationId?: string;
  createdAt: string; // ISO date string
  total: number;
}

export interface ProductSummaryDto {
  id: string;
  name: string;
}

export interface LocationSummaryDto {
  id: string;
  name: string;
}

// Inventory DTOs
export interface InventorySummaryDto {
  product: ProductSummaryDto;
  location: LocationSummaryDto;
  quantity: number;
}

// Stock Movement View Models
export interface StockMovementLogState {
  movements: StockMovementDto[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}

export interface DataGridColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
}

export interface MovementTypeDisplay {
  label: string;
  color: string;
  icon: string;
}

// Inventory View Models
export interface InventorySummaryState {
  inventory: InventorySummaryDto[];
  products: ProductSummaryDto[];
  locations: LocationSummaryDto[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  filters: InventoryFilters;
}

export interface InventoryFilters {
  productId?: string;
  locationId?: string;
  lowStock: boolean;
}

export type UserRole = 'Owner' | 'Member' | 'Viewer';

export type StockOperationType = 'Add' | 'Withdraw' | 'Move' | 'Reconcile' | 'MoveAdd' | 'MoveSubtract';

export interface InventoryCardProps {
  inventory: InventorySummaryDto;
  userRole: UserRole;
  onMove: (inventory: InventorySummaryDto) => void;
  onWithdraw: (inventory: InventorySummaryDto) => void;
  onReconcile: (inventory: InventorySummaryDto) => void;
}

// Stock Movement Creation DTOs
export interface CreateStockMovementCommand {
  productTemplateId: string;
  movementType: StockOperationType;
  delta: number;
  locationId?: string;
  fromLocationId?: string;
  toLocationId?: string;
}

export interface StockOperationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  inventory?: InventorySummaryDto;
}

// Profile View Types
export interface DisplayNameFormData {
  displayName: string;
}

export interface PasswordChangeFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface PasswordStrength {
  score: number; // 0-4
  label: 'Very Weak' | 'Weak' | 'Fair' | 'Good' | 'Strong';
  color: 'red' | 'orange' | 'yellow' | 'blue' | 'green';
  feedback: string[];
}

export interface UserProfileDto {
  id: string;
  email: string;
  displayName: string;
}

export interface ProfilePageState {
  user: UserProfileDto | null;
  accountCreatedAt: string | null;
  displayNameLoading: boolean;
  passwordChangeLoading: boolean;
  displayNameError: string | null;
  passwordChangeError: string | null;
  displayNameSuccess: string | null;
  passwordChangeSuccess: string | null;
}

// Organization Management Types
export interface OrganizationMemberDto {
  userId: string | null;
  email: string;
  role: 'Owner' | 'Member' | 'Viewer';
  status: 'Accepted' | 'Pending';
}

export interface InvitationDto {
  id: string;
  invitedUserId: string;
  role: 'Owner' | 'Member' | 'Viewer';
  status: 'Pending' | 'Accepted' | 'Declined';
}

export interface UserInvitationDto {
  id: string;
  organizationName: string;
  role: 'Owner' | 'Member' | 'Viewer';
  invitedAt: string; // ISO date string
}

export interface UserSearchResult {
  id: string;
  email: string;
  displayName: string;
}

export interface CreateInvitationRequestDto {
  invitedUserId: string;
  role: 'Member' | 'Viewer';
}

export interface UpdateOrganizationRequestDto {
  name: string;
}

// My Invitations State
export interface MyInvitationsState {
  invitations: UserInvitationDto[];
  loading: boolean;
  error: string | null;
  processingInvitations: Set<string>; // IDs of invitations being processed
}

// Organization Settings State
export interface OrganizationSettingsState {
  organization: OrganizationVM | null;
  members: OrganizationMemberDto[];
  invitations: InvitationDto[];
  loading: boolean;
  error: string | null;
  membersPagination: PaginationDto;
  invitationsPagination: PaginationDto;
  currentMembersPage: number;
  currentInvitationsPage: number;
  pageSize: number;
  isInviteModalOpen: boolean;
  inviteLoading: boolean;
  inviteError: string | null;
}

// Component Props
export interface InvitationCardProps {
  invitation: UserInvitationDto;
  loading: boolean;
  disabled: boolean;
}

export interface MemberCardProps {
  member: OrganizationMemberDto;
  onRemove: (userId: string) => void;
  currentUserId: string;
  loading: boolean;
}

export interface InviteUserModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateInvitationRequestDto) => void;
  loading: boolean;
  error: string | null;
}

export interface RoleDisplay {
  label: string;
  color: string;
  description: string;
}

export type InvitationAction = 'accept' | 'decline';