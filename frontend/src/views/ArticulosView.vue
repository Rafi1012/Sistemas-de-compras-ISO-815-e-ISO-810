<script setup>
import { onMounted, ref, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useArticulosStore } from '../stores/articulosStore'
import { useUnidadesMedidaStore } from '../stores/unidadesMedidaStore'
import { Plus, Edit2, Trash2, X, Search } from '@lucide/vue'

const store = useArticulosStore()
const { articulos, isLoading } = storeToRefs(store)

const unidadesStore = useUnidadesMedidaStore()
const { unidades } = storeToRefs(unidadesStore)

onMounted(async () => {
  store.fetchArticulos()
  if (unidades.value.length === 0) {
    unidadesStore.fetchUnidades()
  }
})

const searchQuery = ref('')
const filteredArticulos = computed(() => {
  if (!searchQuery.value) return articulos.value
  const q = searchQuery.value.toLowerCase()
  return articulos.value.filter(a => 
    a.descripcion.toLowerCase().includes(q) ||
    a.marca.toLowerCase().includes(q) ||
    a.estado.toLowerCase().includes(q) ||
    a.id.toString().includes(q) ||
    getUnidadNombre(a.unidadMedidaId).toLowerCase().includes(q)
  )
})

const showModal = ref(false)
const isEditing = ref(false)
const form = ref({ id: null, descripcion: '', marca: '', unidadMedidaId: '', existencia: 0, estado: 'Activo' })

const openModal = (item = null) => {
  if (item) {
    isEditing.value = true
    form.value = { ...item }
  } else {
    isEditing.value = false
    form.value = { id: null, descripcion: '', marca: '', unidadMedidaId: '', existencia: 0, estado: 'Activo' }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const bloquearSignoMenos = (event) => {
  if (event.key === '-') {
    event.preventDefault()
  }
}

const bloquearNegativo = (campo) => {
  const valor = Number(form.value[campo])
  if (!Number.isNaN(valor) && valor < 0) {
    form.value[campo] = 0
  }
}

const save = async () => {
  if (form.value.existencia <= 0) {
    alert("La existencia debe ser mayor a 0.")
    return
  }

  if (isEditing.value) {
    await store.editArticulo(form.value.id, form.value)
  } else {
    await store.addArticulo({ 
      descripcion: form.value.descripcion, 
      marca: form.value.marca,
      unidadMedidaId: form.value.unidadMedidaId,
      existencia: form.value.existencia,
      estado: form.value.estado 
    })
  }
  closeModal()
}

const remove = async (id) => {
  if (confirm('¿Está seguro de eliminar este artículo?')) {
    try {
      await store.removeArticulo(id)
    } catch (error) {
      alert(error.response?.data?.message || "Error al eliminar el artículo.")
    }
  }
}

const getUnidadNombre = (id) => {
  const unidad = unidades.value.find(u => u.id === id)
  return unidad ? unidad.descripcion : 'N/A'
}
</script>

<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Artículos</h1>
      <button @click="openModal()" class="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg font-medium transition-colors shadow-sm">
        <Plus class="w-5 h-5" /> Nuevo Artículo
      </button>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="text-center py-10">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-emerald-600 mx-auto"></div>
    </div>

    <!-- Table -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="p-4 border-b border-gray-100 flex items-center gap-3">
        <div class="relative w-full max-w-md">
          <Search class="w-5 h-5 absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input type="text" v-model="searchQuery" placeholder="Buscar artículos..." class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
      </div>
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-gray-50 text-gray-600 text-sm border-b border-gray-100">
            <th class="py-3 px-6 font-semibold">ID</th>
            <th class="py-3 px-6 font-semibold">Descripción</th>
            <th class="py-3 px-6 font-semibold">Marca</th>
            <th class="py-3 px-6 font-semibold">Unidad</th>
            <th class="py-3 px-6 font-semibold">Existencia</th>
            <th class="py-3 px-6 font-semibold">Estado</th>
            <th class="py-3 px-6 font-semibold text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredArticulos" :key="item.id" class="border-b border-gray-50 hover:bg-gray-50 transition-colors">
            <td class="py-3 px-6 text-gray-600">{{ item.id }}</td>
            <td class="py-3 px-6 font-medium text-gray-800">{{ item.descripcion }}</td>
            <td class="py-3 px-6 font-medium text-gray-600">{{ item.marca }}</td>
            <td class="py-3 px-6 font-medium text-gray-600">{{ getUnidadNombre(item.unidadMedidaId) }}</td>
            <td class="py-3 px-6 font-medium text-gray-800">{{ item.existencia }}</td>
            <td class="py-3 px-6">
              <span :class="['px-3 py-1 rounded-full text-xs font-medium', item.estado === 'Activo' ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700']">
                {{ item.estado }}
              </span>
            </td>
            <td class="py-3 px-6 text-right">
              <button @click="openModal(item)" class="text-blue-600 hover:text-blue-800 mr-3 p-1 rounded-md hover:bg-blue-50 transition-colors inline-block"><Edit2 class="w-4 h-4" /></button>
              <button @click="remove(item.id)" class="text-red-600 hover:text-red-800 p-1 rounded-md hover:bg-red-50 transition-colors inline-block"><Trash2 class="w-4 h-4" /></button>
            </td>
          </tr>
          <tr v-if="articulos.length === 0"><td colspan="7" class="py-8 text-center text-gray-500">No hay artículos registrados.</td></tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 transition-opacity">
      <div class="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-100 flex justify-between items-center">
          <h3 class="text-lg font-bold text-gray-800">{{ isEditing ? 'Editar Artículo' : 'Nuevo Artículo' }}</h3>
          <button @click="closeModal" class="text-gray-400 hover:text-gray-600"><X class="w-5 h-5" /></button>
        </div>
        <form @submit.prevent="save" class="p-6">
          <div class="grid grid-cols-2 gap-4 mb-4">
            <div class="col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
              <input v-model="form.descripcion" type="text" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Marca</label>
              <input v-model="form.marca" type="text" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Unidad Medida</label>
              <select v-model="form.unidadMedidaId" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="" disabled>Seleccione...</option>
                <option v-for="u in unidades" :key="u.id" :value="u.id">{{ u.descripcion }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Existencia</label>
              <input v-model="form.existencia" @input="bloquearNegativo('existencia')" @keydown="bloquearSignoMenos" type="number" min="0.01" step="0.01" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Estado</label>
              <select v-model="form.estado" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="Activo">Activo</option>
                <option value="Inactivo">Inactivo</option>
              </select>
            </div>
          </div>
          <div class="flex justify-end gap-3 mt-6">
            <button type="button" @click="closeModal" class="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg font-medium">Cancelar</button>
            <button type="submit" class="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-medium">Guardar</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
