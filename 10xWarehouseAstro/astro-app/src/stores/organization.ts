import { defineStore } from 'pinia';
import type { OrganizationDto } from '@/types/dto';

export const useOrganizationStore = defineStore('organization', {
  state: () => ({
    organizations: [] as OrganizationDto[],
  }),
  actions: {
    addOrganization(organization: OrganizationDto) {
      this.organizations.push(organization);
    },
  },
});
