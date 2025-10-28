import { createRouter, createWebHistory, type Router } from 'vue-router'

let clientRouter: Router | null = null

if (!import.meta.env.SSR) {
  clientRouter = createRouter({
    history: createWebHistory(),
    routes: [
      {
        path: '/',
        component: () => import('../components/DashboardView.vue'),
      },
      {
        path: '/inventory',
        component: () => import('../components/InventorySummaryView.vue'),
      },
      {
        path: '/warehouses',
        component: () => import('../components/WarehousePageView.vue'),
      },
      {
        path: '/warehouses/:id',
        component: () => import('../components/WarehouseDetailsView.vue'),
      },
      {
        path: '/products',
        component: () => import('../components/ProductPageView.vue'),
      },
      {
        path: '/movements',
        component: () => import('../components/StockMovementLogView.vue'),
      },
      {
        path: '/invitations',
        component: () => import('../components/invitations/InvitationsView.vue'),
      },
      {
        path: '/profile',
        component: () => import('../components/ProfileView.vue'),
      },
      {
        path: '/settings/organization',
        component: () => import('../components/organization/OrganizationSettingsView.vue'),
      },
    ],
  })

  // Add navigation guard to protect authenticated routes
  clientRouter.beforeEach(async (to, from, next) => {
    // Public routes that don't require authentication
    const publicRoutes = ['/login', '/register']
    const isPublicRoute = publicRoutes.includes(to.path)

    // Try to import and use the auth store
    try {
      const { useAuthStore } = await import('@/stores/auth')
      const authStore = useAuthStore()

      // Initialize auth if not already initialized
      if (!authStore.isInitialized) {
        await authStore.initializeAuth()
      }

      // If not authenticated and trying to access a protected route
      if (!isPublicRoute && !authStore.isAuthenticated) {
        // Use window.location for Astro pages
        window.location.href = '/login'
        return
      }

      // If authenticated and trying to access login/register, redirect to home
      if (isPublicRoute && authStore.isAuthenticated) {
        next('/')
        return
      }

      next()
    } catch (error) {
      console.error('Error in router guard:', error)
      // On error, allow navigation but redirect to login if not public
      if (!isPublicRoute) {
        window.location.href = '/login'
      } else {
        next()
      }
    }
  })
}

export { clientRouter }

