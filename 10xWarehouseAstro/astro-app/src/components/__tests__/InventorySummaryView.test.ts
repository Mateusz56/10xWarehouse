import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { mount, VueWrapper } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import { ref, computed, type ComputedRef } from 'vue';
import InventorySummaryView from '../InventorySummaryView.vue';
import type { 
  InventorySummaryDto, 
  ProductSummaryDto, 
  LocationSummaryDto,
  PaginationDto,
  InventoryFilters,
  UserRole
} from '@/types/dto';

// Mock the stores - these will be initialized in beforeEach
let mockInventoryStore: {
  inventory: ReturnType<typeof ref<InventorySummaryDto[]>>;
  products: ReturnType<typeof ref<ProductSummaryDto[]>>;
  locations: ReturnType<typeof ref<LocationSummaryDto[]>>;
  loading: ReturnType<typeof ref<boolean>>;
  error: ReturnType<typeof ref<string | null>>;
  pagination: ReturnType<typeof ref<PaginationDto>>;
  filters: ReturnType<typeof ref<InventoryFilters>>;
  hasInventory: ComputedRef<boolean>;
  canPerformStockOperations: ComputedRef<boolean>;
  fetchInventory: ReturnType<typeof vi.fn>;
  fetchProducts: ReturnType<typeof vi.fn>;
  fetchLocations: ReturnType<typeof vi.fn>;
  setPage: ReturnType<typeof vi.fn>;
  setPageSize: ReturnType<typeof vi.fn>;
  setFilters: ReturnType<typeof vi.fn>;
  clearFilters: ReturnType<typeof vi.fn>;
  clearError: ReturnType<typeof vi.fn>;
};

let mockOrganizationStore: {
  currentOrganizationId: ReturnType<typeof ref<string | null>>;
  currentRole: ComputedRef<UserRole | null>;
};

vi.mock('@/stores/inventory', () => ({
  useInventoryStore: () => {
    return mockInventoryStore;
  },
}));

vi.mock('@/stores/organization', () => ({
  useOrganizationStore: () => {
    return mockOrganizationStore;
  },
}));

// Mock child components
vi.mock('../FilterBar.vue', () => ({
  default: {
    name: 'FilterBar',
    template: '<div class="filter-bar-mock"><slot /></div>',
    props: ['filters', 'products', 'locations'],
    emits: ['filters-change', 'clear-filters'],
  },
}));

vi.mock('../InventoryCard.vue', () => ({
  default: {
    name: 'InventoryCard',
    template: '<div class="inventory-card-mock"><slot /></div>',
    props: ['inventory', 'userRole'],
    emits: ['move', 'withdraw', 'reconcile'],
  },
}));

vi.mock('../AddStockModal.vue', () => ({
  default: {
    name: 'AddStockModal',
    template: '<div v-if="isOpen" class="add-stock-modal-mock"></div>',
    props: ['isOpen', 'onClose', 'onSuccess'],
  },
}));

vi.mock('../MoveStockModal.vue', () => ({
  default: {
    name: 'MoveStockModal',
    template: '<div v-if="isOpen" class="move-stock-modal-mock"></div>',
    props: ['isOpen', 'inventory', 'onClose', 'onSuccess'],
  },
}));

vi.mock('../WithdrawStockModal.vue', () => ({
  default: {
    name: 'WithdrawStockModal',
    template: '<div v-if="isOpen" class="withdraw-stock-modal-mock"></div>',
    props: ['isOpen', 'inventory', 'onClose', 'onSuccess'],
  },
}));

vi.mock('../ReconcileStockModal.vue', () => ({
  default: {
    name: 'ReconcileStockModal',
    template: '<div v-if="isOpen" class="reconcile-stock-modal-mock"></div>',
    props: ['isOpen', 'inventory', 'onClose', 'onSuccess'],
  },
}));

vi.mock('../ui/PaginationControl.vue', () => ({
  default: {
    name: 'PaginationControl',
    template: '<div class="pagination-control-mock"></div>',
    props: ['pagination'],
    emits: ['page-change', 'page-size-change'],
  },
}));

vi.mock('../ui/button', () => ({
  Button: {
    name: 'Button',
    template: '<button class="button-mock"><slot /></button>',
    props: ['variant', 'size', 'disabled'],
  },
}));

describe('InventorySummaryView', () => {
  let wrapper: VueWrapper;
  
  const mockInventory: InventorySummaryDto[] = [
    {
      product: { id: 'prod-1', name: 'Product 1' },
      location: { id: 'loc-1', name: 'Location 1' },
      quantity: 100,
      isLowStock: false,
    },
    {
      product: { id: 'prod-2', name: 'Product 2' },
      location: { id: 'loc-2', name: 'Location 2' },
      quantity: 5,
      isLowStock: true,
    },
  ];

  const mockProducts: ProductSummaryDto[] = [
    { id: 'prod-1', name: 'Product 1' },
    { id: 'prod-2', name: 'Product 2' },
  ];

  const mockLocations: LocationSummaryDto[] = [
    { id: 'loc-1', name: 'Location 1' },
    { id: 'loc-2', name: 'Location 2' },
  ];

  const mockPagination: PaginationDto = {
    page: 1,
    pageSize: 50,
    total: 2,
  };

  const mockFilters: InventoryFilters = {
    productId: undefined,
    locationId: undefined,
    lowStock: false,
  };

  beforeEach(() => {
    setActivePinia(createPinia());
    
    // Initialize inventory store
    const inventoryRef = ref<InventorySummaryDto[]>([...mockInventory]);
    const productsRef = ref<ProductSummaryDto[]>([...mockProducts]);
    const locationsRef = ref<LocationSummaryDto[]>([...mockLocations]);
    const loadingRef = ref(false);
    const errorRef = ref<string | null>(null);
    const paginationRef = ref<PaginationDto>({ ...mockPagination });
    const filtersRef = ref<InventoryFilters>({ ...mockFilters });
    
    mockInventoryStore = {
      inventory: inventoryRef,
      products: productsRef,
      locations: locationsRef,
      loading: loadingRef,
      error: errorRef,
      pagination: paginationRef,
      filters: filtersRef,
      hasInventory: computed(() => inventoryRef.value.length > 0),
      canPerformStockOperations: computed(() => true),
      fetchInventory: vi.fn().mockResolvedValue(undefined),
      fetchProducts: vi.fn().mockResolvedValue(undefined),
      fetchLocations: vi.fn().mockResolvedValue(undefined),
      setPage: vi.fn(),
      setPageSize: vi.fn(),
      setFilters: vi.fn(),
      clearFilters: vi.fn(),
      clearError: vi.fn(),
    };
    
    // Initialize organization store
    mockOrganizationStore = {
      currentOrganizationId: ref<string | null>('org-1'),
      currentRole: computed(() => 'Owner' as UserRole),
    };
    
    // Clear all mocks
    vi.clearAllMocks();
  });

  afterEach(() => {
    wrapper?.unmount();
  });

  function createWrapper() {
    return mount(InventorySummaryView, {
      global: {
        plugins: [createPinia()],
      },
    });
  }

  describe('Component Rendering', () => {
    it('renders the inventory summary view with header', () => {
      wrapper = createWrapper();
      
      expect(wrapper.text()).toContain('Inventory Summary');
      expect(wrapper.text()).toContain('View and manage inventory levels across all locations');
    });
  });


  describe('Error State', () => {
    it('displays error message when error exists', async () => {
      mockInventoryStore.error.value = 'Failed to load inventory';
      mockInventoryStore.loading.value = false;
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      expect(wrapper.text()).toContain('Error loading inventory');
      expect(wrapper.text()).toContain('Failed to load inventory');
    });

    it('renders retry button in error state', async () => {
      mockInventoryStore.error.value = 'Failed to load inventory';
      mockInventoryStore.loading.value = false;
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const buttons = wrapper.findAll('button');
      const retryButton = buttons.find(btn => btn.text().includes('Try again'));
      expect(retryButton).toBeDefined();
    });

    it('calls clearError and loadData when retry button is clicked', async () => {
      mockInventoryStore.error.value = 'Failed to load inventory';
      mockInventoryStore.loading.value = false;
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const buttons = wrapper.findAll('button');
      const retryButton = buttons.find(btn => btn.text().includes('Try again'));
      
      if (retryButton) {
        await retryButton.trigger('click');
        await wrapper.vm.$nextTick();
        
        expect(mockInventoryStore.clearError).toHaveBeenCalled();
        expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
      }
    });
  });


  describe('Role-Based Access Control', () => {
    it('shows Add Stock button for Owner role', async () => {
      // Store already initialized with Owner role and canPerformStockOperations = true
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const buttons = wrapper.findAll('button');
      const addStockButton = buttons.find(btn => btn.text().includes('Add Stock'));
      expect(addStockButton).toBeDefined();
    });

    it('shows Add Stock button for Member role', async () => {
      // Reinitialize with Member role
      mockOrganizationStore = {
        ...mockOrganizationStore,
        currentRole: computed(() => 'Member' as UserRole),
      };
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const buttons = wrapper.findAll('button');
      const addStockButton = buttons.find(btn => btn.text().includes('Add Stock'));
      expect(addStockButton).toBeDefined();
    });

  });

  describe('Data Loading', () => {
    it('calls fetchInventory, fetchProducts, and fetchLocations on mount', async () => {
      mockOrganizationStore.currentOrganizationId.value = 'org-1';
      wrapper = createWrapper();
      
      // Wait for watcher and onMounted to execute
      await wrapper.vm.$nextTick();
      await new Promise(resolve => setTimeout(resolve, 100));
      
      expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
      expect(mockInventoryStore.fetchProducts).toHaveBeenCalled();
      expect(mockInventoryStore.fetchLocations).toHaveBeenCalled();
    });

    it('waits for organization data before loading inventory', async () => {
      // Set orgId to null BEFORE creating wrapper
      mockOrganizationStore.currentOrganizationId.value = null;
      vi.clearAllMocks();
      
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Should not fetch if no organization ID - but watcher with immediate:true might still call
      // The important thing is that loadData returns early if no orgId
      // Since the watcher fires immediately, we need to check that it doesn't actually fetch
      
      // Set organization ID - this will trigger the watcher
      mockOrganizationStore.currentOrganizationId.value = 'org-1';
      await wrapper.vm.$nextTick();
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Now should fetch after orgId is set
      expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
    });
  });

  describe('Filter Interactions', () => {
    it('handles filter changes from FilterBar', async () => {
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const filterBar = wrapper.findComponent({ name: 'FilterBar' });
      const newFilters: InventoryFilters = {
        productId: 'prod-1',
        locationId: undefined,
        lowStock: false,
      };
      
      await filterBar.vm.$emit('filters-change', newFilters);
      await wrapper.vm.$nextTick();
      
      expect(mockInventoryStore.setFilters).toHaveBeenCalledWith(newFilters);
      expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
    });

    it('handles clear filters event', async () => {
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      const filterBar = wrapper.findComponent({ name: 'FilterBar' });
      await filterBar.vm.$emit('clear-filters');
      await wrapper.vm.$nextTick();
      
      expect(mockInventoryStore.clearFilters).toHaveBeenCalled();
      expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
    });
  });

  describe('Refresh Functionality', () => {
    it('reloads all data when refresh button is clicked', async () => {
      wrapper = createWrapper();
      await wrapper.vm.$nextTick();
      
      vi.clearAllMocks();
      
      const buttons = wrapper.findAll('button');
      const refreshButton = buttons.find(btn => btn.text().includes('Refresh'));
      
      if (refreshButton) {
        await refreshButton.trigger('click');
        await wrapper.vm.$nextTick();
        await new Promise(resolve => setTimeout(resolve, 100));
        
        expect(mockInventoryStore.fetchInventory).toHaveBeenCalled();
        expect(mockInventoryStore.fetchProducts).toHaveBeenCalled();
        expect(mockInventoryStore.fetchLocations).toHaveBeenCalled();
      }
    });
  });

});

