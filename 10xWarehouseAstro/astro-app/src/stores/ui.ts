import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { WarehouseVM, WarehouseWithLocationsDto, LocationVM } from '@/types/dto';

export const useUiStore = defineStore('ui', () => {
  // Organization modals
  const isCreateOrganizationModalOpen = ref(false);

  function openCreateOrganizationModal() {
    isCreateOrganizationModalOpen.value = true;
  }

  function closeCreateOrganizationModal() {
    isCreateOrganizationModalOpen.value = false;
  }

  // Warehouse modals
  const isCreateWarehouseModalOpen = ref(false);
  const isEditWarehouseModalOpen = ref(false);
  const isDeleteWarehouseModalOpen = ref(false);
  const selectedWarehouse = ref<WarehouseVM | null>(null);
  const selectedWarehouseDetails = ref<WarehouseWithLocationsDto | null>(null);

  // Location modals
  const isCreateLocationModalOpen = ref(false);
  const isEditLocationModalOpen = ref(false);
  const isDeleteLocationModalOpen = ref(false);
  const selectedLocation = ref<LocationVM | null>(null);

  function openCreateWarehouseModal() {
    isCreateWarehouseModalOpen.value = true;
  }

  function closeCreateWarehouseModal() {
    isCreateWarehouseModalOpen.value = false;
  }

  function openEditWarehouseModal(warehouse: WarehouseVM) {
    selectedWarehouse.value = warehouse;
    isEditWarehouseModalOpen.value = true;
  }

  function closeEditWarehouseModal() {
    isEditWarehouseModalOpen.value = false;
    selectedWarehouse.value = null;
  }

  function openEditWarehouseDetailsModal(warehouse: WarehouseWithLocationsDto) {
    selectedWarehouseDetails.value = warehouse;
    isEditWarehouseModalOpen.value = true;
  }

  function closeEditWarehouseDetailsModal() {
    isEditWarehouseModalOpen.value = false;
    selectedWarehouseDetails.value = null;
  }

  function openDeleteWarehouseModal(warehouse: WarehouseVM) {
    selectedWarehouse.value = warehouse;
    isDeleteWarehouseModalOpen.value = true;
  }

  function closeDeleteWarehouseModal() {
    isDeleteWarehouseModalOpen.value = false;
    selectedWarehouse.value = null;
  }

  function openDeleteWarehouseDetailsModal(warehouse: WarehouseWithLocationsDto) {
    selectedWarehouseDetails.value = warehouse;
    isDeleteWarehouseModalOpen.value = true;
  }

  function closeDeleteWarehouseDetailsModal() {
    isDeleteWarehouseModalOpen.value = false;
    selectedWarehouseDetails.value = null;
  }

  // Location modal functions
  function openCreateLocationModal() {
    isCreateLocationModalOpen.value = true;
  }

  function closeCreateLocationModal() {
    isCreateLocationModalOpen.value = false;
  }

  function openEditLocationModal(location: LocationVM) {
    selectedLocation.value = location;
    isEditLocationModalOpen.value = true;
  }

  function closeEditLocationModal() {
    isEditLocationModalOpen.value = false;
    selectedLocation.value = null;
  }

  function openDeleteLocationModal(location: LocationVM) {
    selectedLocation.value = location;
    isDeleteLocationModalOpen.value = true;
  }

  function closeDeleteLocationModal() {
    isDeleteLocationModalOpen.value = false;
    selectedLocation.value = null;
  }

  return {
    // Organization modals
    isCreateOrganizationModalOpen,
    openCreateOrganizationModal,
    closeCreateOrganizationModal,
    
    // Warehouse modals
    isCreateWarehouseModalOpen,
    isEditWarehouseModalOpen,
    isDeleteWarehouseModalOpen,
    selectedWarehouse,
    selectedWarehouseDetails,
    openCreateWarehouseModal,
    closeCreateWarehouseModal,
    openEditWarehouseModal,
    closeEditWarehouseModal,
    openEditWarehouseDetailsModal,
    closeEditWarehouseDetailsModal,
    openDeleteWarehouseModal,
    closeDeleteWarehouseModal,
    openDeleteWarehouseDetailsModal,
    closeDeleteWarehouseDetailsModal,
    
    // Location modals
    isCreateLocationModalOpen,
    isEditLocationModalOpen,
    isDeleteLocationModalOpen,
    selectedLocation,
    openCreateLocationModal,
    closeCreateLocationModal,
    openEditLocationModal,
    closeEditLocationModal,
    openDeleteLocationModal,
    closeDeleteLocationModal,
  };
});
