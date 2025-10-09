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
}

export interface NavItemVM {
  to: string;
  label: string;
  icon: string;
  requiredRole?: 'Owner';
}