import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { useAuthStore } from '../stores/authStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/departamentos',
      name: 'departamentos',
      component: () => import('../views/DepartamentosView.vue'),
    },
    {
      path: '/unidades-medida',
      name: 'unidades-medida',
      component: () => import('../views/UnidadesMedidaView.vue'),
    },
    {
      path: '/proveedores',
      name: 'proveedores',
      component: () => import('../views/ProveedoresView.vue'),
    },
    {
      path: '/articulos',
      name: 'articulos',
      component: () => import('../views/ArticulosView.vue'),
    },
    {
      path: '/ordenes-compra',
      name: 'ordenes-compra',
      component: () => import('../views/OrdenesCompraView.vue'),
    },
    {
      path: '/consulta',
      name: 'consulta',
      component: () => import('../views/ConsultaView.vue'),
    },
    {
      path: '/empleados',
      name: 'empleados',
      component: () => import('../views/EmpleadosView.vue'),
    },
    {
      path: '/asientos-contables',
      name: 'asientos-contables',
      component: () => import('../views/AsientosContablesView.vue'),
    }
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  if (!to.meta.public && !authStore.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.public && authStore.isAuthenticated) {
    return { name: 'home' }
  }
})

export default router
