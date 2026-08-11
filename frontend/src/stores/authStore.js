import { defineStore } from 'pinia'
import { login as loginRequest, register as registerRequest } from '../services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    usuario: JSON.parse(localStorage.getItem('usuario') || 'null'),
    error: null,
    isLoading: false
  }),
  getters: {
    isAuthenticated: (state) => !!state.token
  },
  actions: {
    setSesion(data) {
      this.token = data.token
      this.usuario = { id: data.usuarioId, nombreUsuario: data.nombreUsuario, email: data.email }
      localStorage.setItem('token', data.token)
      localStorage.setItem('usuario', JSON.stringify(this.usuario))
    },
    async login(nombreUsuario, password) {
      this.isLoading = true
      this.error = null
      try {
        const data = await loginRequest(nombreUsuario, password)
        this.setSesion(data)
        return true
      } catch (err) {
        this.error = err.response?.data?.message || 'Usuario o contraseña incorrectos.'
        return false
      } finally {
        this.isLoading = false
      }
    },
    async register(nombreUsuario, email, password) {
      this.isLoading = true
      this.error = null
      try {
        const data = await registerRequest(nombreUsuario, email, password)
        this.setSesion(data)
        return true
      } catch (err) {
        this.error = err.response?.data?.message || 'No se pudo completar el registro.'
        return false
      } finally {
        this.isLoading = false
      }
    },
    logout() {
      this.token = null
      this.usuario = null
      localStorage.removeItem('token')
      localStorage.removeItem('usuario')
    }
  }
})
