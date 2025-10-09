import { defineStore } from 'pinia';

export const useUiStore = defineStore('ui', {
  state: () => ({
    isCreateOrganizationModalOpen: false,
  }),
  actions: {
    openCreateOrganizationModal() {
      this.isCreateOrganizationModalOpen = true;
    },
    closeCreateOrganizationModal() {
      this.isCreateOrganizationModalOpen = false;
    },
  },
});
