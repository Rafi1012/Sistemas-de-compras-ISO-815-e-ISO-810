<script setup>
import { ref, computed } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { UserPlus } from '@lucide/vue'

const authStore = useAuthStore()
const router = useRouter()

const nombreUsuario = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')

const passwordsCoinciden = computed(() => password.value === confirmPassword.value)

const submit = async () => {
  if (!passwordsCoinciden.value) {
    authStore.error = 'Las contraseñas no coinciden.'
    return
  }
  const ok = await authStore.register(nombreUsuario.value, email.value, password.value)
  if (ok) {
    router.push('/')
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
        <p class="text-gray-500 text-sm mt-1">Crea tu cuenta</p>
      </div>

      <form @submit.prevent="submit" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Usuario</label>
          <input v-model="nombreUsuario" type="text" required autofocus
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Correo</label>
          <input v-model="email" type="email" required
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
          <input v-model="password" type="password" required minlength="6"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Confirmar Contraseña</label>
          <input v-model="confirmPassword" type="password" required minlength="6"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>

        <p v-if="authStore.error" class="text-sm text-red-600">{{ authStore.error }}</p>

        <button type="submit" :disabled="authStore.isLoading"
          class="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white px-4 py-2 rounded-lg font-medium transition-colors">
          <UserPlus class="w-4 h-4" /> {{ authStore.isLoading ? 'Creando cuenta...' : 'Registrarme' }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-500 mt-6">
        ¿Ya tienes cuenta?
        <RouterLink to="/login" class="text-emerald-600 hover:text-emerald-700 font-medium">Inicia sesión</RouterLink>
      </p>
    </div>
  </div>
</template>
