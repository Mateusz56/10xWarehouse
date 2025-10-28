import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { 
  ProductTemplateDto, 
  ProductTemplateVM, 
  CreateProductTemplateRequestDto, 
  PaginationDto 
} from '@/types/dto';
import { productTemplateApi } from '@/lib/api';
import { useOrganizationStore } from '@/stores/organization';

export const useProductStore = defineStore('product', () => {
  const organizationStore = useOrganizationStore();
  
  // State
  const products = ref<ProductTemplateVM[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const pagination = ref<PaginationDto>({
    page: 1,
    pageSize: 10,
    total: 0
  });
  const currentPage = ref(1);
  const pageSize = ref(10);
  const searchQuery = ref('');

  // Computed
  const hasProducts = computed(() => products.value.length > 0);
  const isEmpty = computed(() => !loading.value && products.value.length === 0);
  const canCreateProduct = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner';
  });
  const canEditProduct = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner';
  });
  const canDeleteProduct = computed(() => {
    const role = organizationStore.currentRole;
    return role === 'Owner';
  });

  // Actions
  async function fetchProducts(page?: number, size?: number, search?: string): Promise<void> {
    if (!organizationStore.currentOrganizationId) {
      error.value = 'No organization selected';
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const targetPage = page ?? currentPage.value;
      const targetPageSize = size ?? pageSize.value;
      const targetSearch = search ?? searchQuery.value;
      
      // Validate pagination parameters
      if (targetPage < 1) {
        throw new Error('Page number must be greater than 0');
      }
      if (targetPageSize < 1 || targetPageSize > 100) {
        throw new Error('Page size must be between 1 and 100');
      }
      
      const response = await productTemplateApi.getProductTemplates(
        organizationStore.currentOrganizationId,
        targetPage,
        targetPageSize,
        targetSearch || undefined
      );

      products.value = response.data.map(product => ({
        id: product.id,
        name: product.name,
        barcode: product.barcode,
        description: product.description,
        lowStockThreshold: product.lowStockThreshold
      }));

      // Ensure pagination object has all required properties
      pagination.value = {
        page: response.pagination?.page || targetPage,
        pageSize: response.pagination?.pageSize || targetPageSize,
        total: response.pagination?.total || 0
      };
      
      currentPage.value = targetPage;
      pageSize.value = targetPageSize;
      searchQuery.value = targetSearch;
    } catch (err) {
      console.error('Failed to fetch products:', err);
      error.value = err instanceof Error ? err.message : 'Failed to load products';
      
      // If auth error, sign out user
      if (err instanceof Error && err.message === 'Authentication required') {
        const { useAuthStore } = await import('@/stores/auth');
        const authStore = useAuthStore();
        await authStore.signOut();
      }
    } finally {
      loading.value = false;
    }
  }

  async function createProduct(data: CreateProductTemplateRequestDto): Promise<ProductTemplateDto> {
    if (!organizationStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      // Add organizationId to the data
      const dataWithOrgId = {
        ...data,
        organizationId: organizationStore.currentOrganizationId
      };
      
      const newProduct = await productTemplateApi.createProductTemplate(dataWithOrgId);
      
      // Add to local state
      products.value.unshift({
        id: newProduct.id,
        name: newProduct.name,
        barcode: newProduct.barcode,
        description: newProduct.description,
        lowStockThreshold: newProduct.lowStockThreshold
      });

      // Update pagination
      pagination.value.total += 1;

      return newProduct;
    } catch (err) {
      console.error('Failed to create product:', err);
      error.value = err instanceof Error ? err.message : 'Failed to create product';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function updateProduct(id: string, data: CreateProductTemplateRequestDto): Promise<ProductTemplateDto> {
    if (!organizationStore.currentOrganizationId) {
      throw new Error('No organization selected');
    }

    loading.value = true;
    error.value = null;

    try {
      // Add organizationId to the data
      const dataWithOrgId = {
        ...data,
        organizationId: organizationStore.currentOrganizationId
      };
      
      const updatedProduct = await productTemplateApi.updateProductTemplate(id, dataWithOrgId);
      
      // Update local state
      const index = products.value.findIndex(p => p.id === id);
      if (index !== -1) {
        products.value[index] = {
          ...products.value[index],
          name: updatedProduct.name,
          barcode: updatedProduct.barcode,
          description: updatedProduct.description,
          lowStockThreshold: updatedProduct.lowStockThreshold
        };
      }

      return updatedProduct;
    } catch (err) {
      console.error('Failed to update product:', err);
      error.value = err instanceof Error ? err.message : 'Failed to update product';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function deleteProduct(id: string): Promise<void> {
    loading.value = true;
    error.value = null;

    try {
      await productTemplateApi.deleteProductTemplate(id);
      
      // Remove from local state
      products.value = products.value.filter(p => p.id !== id);
      
      // Update pagination
      pagination.value.total -= 1;

      // If current page is empty and not the first page, go to previous page
      if (products.value.length === 0 && currentPage.value > 1) {
        await fetchProducts(currentPage.value - 1, pageSize.value, searchQuery.value);
      }
    } catch (err) {
      console.error('Failed to delete product:', err);
      error.value = err instanceof Error ? err.message : 'Failed to delete product';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function setPage(page: number): void {
    currentPage.value = page;
  }

  function setPageSize(size: number): void {
    pageSize.value = size;
    currentPage.value = 1; // Reset to first page when changing page size
  }

  function setSearchQuery(query: string): void {
    searchQuery.value = query;
    currentPage.value = 1; // Reset to first page when searching
  }

  function clearError(): void {
    error.value = null;
  }

  function clearProducts(): void {
    products.value = [];
    pagination.value = {
      page: 1,
      pageSize: 10,
      total: 0
    };
    currentPage.value = 1;
    pageSize.value = 10;
    searchQuery.value = '';
    error.value = null;
  }

  return {
    // State
    products,
    loading,
    error,
    pagination,
    currentPage,
    pageSize,
    searchQuery,
    
    // Computed
    hasProducts,
    isEmpty,
    canCreateProduct,
    canEditProduct,
    canDeleteProduct,
    
    // Actions
    fetchProducts,
    createProduct,
    updateProduct,
    deleteProduct,
    setPage,
    setPageSize,
    setSearchQuery,
    clearError,
    clearProducts
  };
});
