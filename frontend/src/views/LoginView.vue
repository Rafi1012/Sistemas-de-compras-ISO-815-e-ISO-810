<script setup>
import { ref } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { LogIn } from '@lucide/vue'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const nombreUsuario = ref('')
const password = ref('')

const submit = async () => {
  const ok = await authStore.login(nombreUsuario.value, password.value)
  if (ok) {
    router.push(route.query.redirect || '/')
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100 px-4">
    <div class="w-full max-w-sm bg-white rounded-2xl shadow-xl p-8">
      <div class="text-center mb-6">
        <h1 class="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-emerald-600 to-cyan-600">
          ComprasApp
        </h1>
        <p class="text-gray-500 text-sm mt-1">Inicia sesión para continuar</p>
      </div>

      <form @submit.prevent="submit" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Usuario</label>
          <input v-model="nombreUsuario" type="text" required autofocus
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
          <input v-model="password" type="password" required
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>

        <p v-if="authStore.error" class="text-sm text-red-600">{{ authStore.error }}</p>

        <button type="submit" :disabled="authStore.isLoading"
          class="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white px-4 py-2 rounded-lg font-medium transition-colors">
          <LogIn class="w-4 h-4" /> {{ authStore.isLoading ? 'Ingresando...' : 'Ingresar' }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-500 mt-6">
        ¿No tienes cuenta?
        <RouterLink to="/register" class="text-emerald-600 hover:text-emerald-700 font-medium">Regístrate</RouterLink>
      </p>
    </div>
  </div>
</template>
