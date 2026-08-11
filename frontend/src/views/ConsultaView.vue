<script setup>
import { onMounted, ref } from 'vue'
import { consultarOrdenesCompra } from '../services/consultasService'
import { useProveedoresStore } from '../stores/proveedoresStore'
import { useDepartamentosStore } from '../stores/departamentosStore'
import { useEmpleadosStore } from '../stores/empleadosStore'
import { Search, FileDown } from '@lucide/vue'
import { storeToRefs } from 'pinia'

const ordenesFiltradas = ref([])
const isLoading = ref(false)

const provStore = useProveedoresStore()
const { proveedores } = storeToRefs(provStore)

const depStore = useDepartamentosStore()
const { departamentos } = storeToRefs(depStore)

const empStore = useEmpleadosStore()
const { empleados } = storeToRefs(empStore)

const filtros = ref({
  fechaDesde: '',
  fechaHasta: '',
  proveedorId: '',
  departamentoId: '',
  empleadoId: '',
  estado: ''
})

onMounted(async () => {
  if (proveedores.value.length === 0) provStore.fetchProveedores()
  if (departamentos.value.length === 0) depStore.fetchDepartamentos()
  if (empleados.value.length === 0) empStore.fetchEmpleados()
  await buscar()
})

const buscar = async () => {
  if (filtros.value.fechaDesde && filtros.value.fechaHasta && filtros.value.fechaDesde > filtros.value.fechaHasta) {
    alert('La fecha "Desde" no puede ser mayor que la fecha "Hasta".')
    return
  }

  isLoading.value = true
  try {
    // Limpiamos los filtros vacios
    const params = {}
    if (filtros.value.fechaDesde) params.fechaDesde = filtros.value.fechaDesde
    if (filtros.value.fechaHasta) params.fechaHasta = filtros.value.fechaHasta
    if (filtros.value.proveedorId) params.proveedorId = filtros.value.proveedorId
    if (filtros.value.departamentoId) params.departamentoId = filtros.value.departamentoId
    if (filtros.value.empleadoId) params.empleadoId = filtros.value.empleadoId
    if (filtros.value.estado) params.estado = filtros.value.estado

    ordenesFiltradas.value = await consultarOrdenesCompra(params)
  } catch (error) {
    console.error("Error consultando órdenes:", error)
  } finally {
    isLoading.value = false
  }
}

const formatCurrency = (val) => new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(val)

const exportCSV = () => {
  const headers = ['No. Orden', 'Fecha', 'Proveedor', 'Departamento', 'Articulo', 'Cantidad', 'Costo Unit', 'Total', 'Estado']
  const rows = ordenesFiltradas.value.map(o => {
    const articuloDesc = o.detalles && o.detalles.length > 0 ? o.detalles[0].articuloDescripcion : 'N/A'
    const cant = o.detalles && o.detalles.length > 0 ? o.detalles[0].cantidad : 0
    const costoUnit = o.detalles && o.detalles.length > 0 ? o.detalles[0].costoUnitario : 0

    return [
      `OC-${o.numero}`,
      new Date(o.fechaOrden).toLocaleDateString(),
      `"${o.proveedorNombre}"`,
      `"${o.departamentoNombre}"`,
      `"${articuloDesc}"`,
      cant,
      costoUnit,
      o.total.toFixed(2),
      o.estado
    ]
  })
  const csvContent = "data:text/csv;charset=utf-8," + [headers.join(','), ...rows.map(e => e.join(','))].join("\n")
  const encodedUri = encodeURI(csvContent)
  const link = document.createElement("a")
  link.setAttribute("href", encodedUri)
  link.setAttribute("download", "consulta_ordenes.csv")
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
</script>

<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Consulta de Órdenes</h1>
      <button @click="exportCSV" class="flex items-center gap-2 bg-slate-800 hover:bg-slate-900 text-white px-4 py-2 rounded-lg font-medium transition-colors shadow-sm">
        <FileDown class="w-5 h-5" /> Exportar a CSV
      </button>
    </div>

    <!-- Filtros -->
    <div class="bg-white p-6 rounded-xl shadow-sm border border-gray-100 mb-6 flex flex-wrap gap-4 items-end">
      <div class="flex-1 min-w-[150px]">
        <label class="block text-sm font-medium text-gray-700 mb-1">Fecha Desde</label>
        <input v-model="filtros.fechaDesde" type="date" :max="filtros.fechaHasta || undefined" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
      </div>
      <div class="flex-1 min-w-[150px]">
        <label class="block text-sm font-medium text-gray-700 mb-1">Fecha Hasta</label>
        <input v-model="filtros.fechaHasta" type="date" :min="filtros.fechaDesde || undefined" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
      </div>
      <div class="flex-1 min-w-[180px]">
        <label class="block text-sm font-medium text-gray-700 mb-1">Proveedor</label>
        <select v-model="filtros.proveedorId" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
          <option value="">Todos</option>
          <option v-for="p in proveedores" :key="p.id" :value="p.id">{{ p.nombreComercial }}</option>
        </select>
      </div>
      <div class="flex-1 min-w-[150px]">
        <label class="block text-sm font-medium text-gray-700 mb-1">Departamento</label>
        <select v-model="filtros.departamentoId" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
          <option value="">Todos</option>
          <option v-for="d in departamentos" :key="d.id" :value="d.id">{{ d.nombre }}</option>
        </select>
      </div>
      <div class="flex-1 min-w-[150px]">
        <label class="block text-sm font-medium text-gray-700 mb-1">Estado</label>
        <select v-model="filtros.estado" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
          <option value="">Todos</option>
          <option value="Pendiente">Pendiente</option>
          <option value="Generada">Generada</option>
          <option value="Aprobada">Aprobada</option>
          <option value="Recibida">Recibida</option>
          <option value="Cancelada">Cancelada</option>
        </select>
      </div>
      <div>
        <button @click="buscar" class="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-6 py-2 rounded-lg font-medium transition-colors shadow-sm">
          <Search class="w-5 h-5" /> Buscar
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="text-center py-10">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-emerald-600 mx-auto"></div>
    </div>

    <!-- Resultados -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden overflow-x-auto">
      <table class="w-full text-left border-collapse min-w-[800px]">
        <thead>
          <tr class="bg-gray-50 text-gray-600 text-sm border-b border-gray-100">
            <th class="py-3 px-6 font-semibold">No. Orden</th>
            <th class="py-3 px-6 font-semibold">Fecha</th>
            <th class="py-3 px-6 font-semibold">Proveedor</th>
            <th class="py-3 px-6 font-semibold">Artículo(s)</th>
            <th class="py-3 px-6 font-semibold text-right">Total</th>
            <th class="py-3 px-6 font-semibold text-center">Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in ordenesFiltradas" :key="item.numero" class="border-b border-gray-50 hover:bg-gray-50 transition-colors">
            <td class="py-3 px-6 font-medium text-gray-800">OC-{{ item.numero }}</td>
            <td class="py-3 px-6 text-gray-600">{{ new Date(item.fechaOrden).toLocaleDateString() }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.proveedorNombre }}</td>
            <td class="py-3 px-6 text-gray-600">
              <span v-if="item.detalles && item.detalles.length > 0">
                {{ item.detalles[0].cantidad }} {{ item.detalles[0].unidadMedidaDescripcion }} de {{ item.detalles[0].articuloDescripcion }}
              </span>
            </td>
            <td class="py-3 px-6 text-right font-medium text-gray-800">{{ formatCurrency(item.total) }}</td>
            <td class="py-3 px-6 text-center">
              <span :class="[
                'px-3 py-1 rounded-full text-xs font-medium',
                item.estado === 'Recibida' ? 'bg-blue-100 text-blue-700' : 
                item.estado === 'Aprobada' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'
              ]">
                {{ item.estado }}
              </span>
            </td>
          </tr>
          <tr v-if="ordenesFiltradas.length === 0"><td colspan="6" class="py-8 text-center text-gray-500">No hay resultados para esta búsqueda.</td></tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
