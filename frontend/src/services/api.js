import axios from 'axios'

// La URL base vendrá de las variables de entorno (.env)
// Por defecto apuntamos a localhost:3000 si no existe.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://sistemadecompras-api-env.eba-md7sxuvd.us-east-1.elasticbeanstalk.com',
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
})

// Interceptores para manejo de errores o tokens si se requiere en el futuro
api.interceptors.response.use(
  response => response,
  error => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

export default api
