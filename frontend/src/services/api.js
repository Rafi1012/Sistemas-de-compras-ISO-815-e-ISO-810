import axios from 'axios'

// La URL base vendrá de las variables de entorno (.env)
// Por defecto apuntamos a localhost:3000 si no existe.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
})

// Adjunta el token JWT (si existe) a cada solicitud
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Si el token es inválido/expiró, limpia la sesión y regresa a login
api.interceptors.response.use(
  response => response,
  error => {
    console.error('API Error:', error)
    const esRutaAuth = error.config?.url?.includes('/auth/')
    if (error.response?.status === 401 && !esRutaAuth) {
      localStorage.removeItem('token')
      localStorage.removeItem('usuario')
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default api
