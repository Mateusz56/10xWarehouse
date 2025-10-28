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
}

export { clientRouter }

