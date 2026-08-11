import api from './api'

const ENDPOINT = '/auth'

export const login = async (nombreUsuario, password) => {
  const response = await api.post(`${ENDPOINT}/login`, { nombreUsuario, password })
  return response.data
}

export const register = async (nombreUsuario, email, password) => {
  const response = await api.post(`${ENDPOINT}/registro`, { nombreUsuario, email, password })
  return response.data
}
