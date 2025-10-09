import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUiStore = defineStore('ui', () => {
  const isCreateOrganizationModalOpen = ref(false);

  function openCreateOrganizationModal() {
    isCreateOrganizationModalOpen.value = true;
  }

  function closeCreateOrganizationModal() {
    isCreateOrganizationModalOpen.value = false;
  }

  return {
    isCreateOrganizationModalOpen,
    openCreateOrganizationModal,
    closeCreateOrganizationModal,
  };
});
