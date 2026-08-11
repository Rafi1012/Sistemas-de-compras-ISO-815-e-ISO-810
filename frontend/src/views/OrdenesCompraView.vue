<script setup>
import { onMounted, ref, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useOrdenesCompraStore } from '../stores/ordenesCompraStore'
import { useArticulosStore } from '../stores/articulosStore'
import { useUnidadesMedidaStore } from '../stores/unidadesMedidaStore'
import { useProveedoresStore } from '../stores/proveedoresStore'
import { useDepartamentosStore } from '../stores/departamentosStore'
import { useEmpleadosStore } from '../stores/empleadosStore'
import { Plus, Send, X, CheckCircle, Check, Search, Ban, ChevronUp, ChevronDown } from '@lucide/vue'

const store = useOrdenesCompraStore()
const { ordenes, isLoading } = storeToRefs(store)

const proveedoresStore = useProveedoresStore()
const { proveedores } = storeToRefs(proveedoresStore)
const depsStore = useDepartamentosStore()
const { departamentos } = storeToRefs(depsStore)
const empleadosStore = useEmpleadosStore()
const { empleados } = storeToRefs(empleadosStore)
const articulosStore = useArticulosStore()
const { articulos } = storeToRefs(articulosStore)
const unidadesStore = useUnidadesMedidaStore()
const { unidades } = storeToRefs(unidadesStore)

onMounted(async () => {
  store.fetchOrdenes()
  if (proveedores.value.length === 0) proveedoresStore.fetchProveedores()
  if (departamentos.value.length === 0) depsStore.fetchDepartamentos()
  if (empleados.value.length === 0) empleadosStore.fetchEmpleados()
  if (articulos.value.length === 0) articulosStore.fetchArticulos()
  if (unidades.value.length === 0) unidadesStore.fetchUnidades()
})

const searchQuery = ref('')
const activeTab = ref('enProceso')

const tabFilteredOrdenes = computed(() => {
  if (activeTab.value === 'enProceso') {
    return ordenes.value.filter(o => o.estado === 'Pendiente' || o.estado === 'Aprobada')
  }
  return ordenes.value.filter(o => o.estado === 'Recibida' || o.estado === 'Cancelada')
})

const filteredOrdenes = computed(() => {
  if (!searchQuery.value) return tabFilteredOrdenes.value
  const q = searchQuery.value.toLowerCase()
  return tabFilteredOrdenes.value.filter(o =>
    o.numero.toString().includes(q) ||
    o.estado.toLowerCase().includes(q) ||
    o.proveedorNombre.toLowerCase().includes(q) ||
    o.departamentoNombre.toLowerCase().includes(q) ||
    (o.detalles && o.detalles.some(d => d.articuloDescripcion.toLowerCase().includes(q)))
  )
})

const showModal = ref(false)
const asientoSuccess = ref(false)
const orderRecibidaId = ref(null)

const form = ref({
  fechaOrden: new Date().toISOString().split('T')[0],
  proveedorId: '',
  departamentoId: '',
  empleadoId: '',
  articuloId: '',
  cantidad: 1,
  unidadMedidaId: '',
  costoUnitario: 0
})

const onArticuloChange = () => {
  const art = articulos.value.find(a => a.id === form.value.articuloId)
  if (art) {
    form.value.unidadMedidaId = art.unidadMedidaId
  }
}

const openModal = () => {
  form.value = {
    fechaOrden: new Date().toISOString().split('T')[0],
    proveedorId: '',
    departamentoId: '',
    empleadoId: '',
    articuloId: '',
    cantidad: 1,
    unidadMedidaId: '',
    costoUnitario: 0
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const aprobar = async (orden) => {
  if (confirm(`¿Está seguro de aprobar la orden ${orden.numero}?`)) {
    try {
      await store.aprobarOrden(orden.numero)
    } catch (e) {
      alert(e.response?.data?.message || "Error al aprobar orden.")
    }
  }
}

const cancelar = async (orden) => {
  if (confirm(`¿Está seguro de cancelar la orden ${orden.numero}?`)) {
    try {
      await store.cancelarOrden(orden.numero)
    } catch (e) {
      alert(e.response?.data?.message || "Error al cancelar orden.")
    }
  }
}

const save = async () => {
  if (form.value.cantidad <= 0 || form.value.costoUnitario <= 0) {
    alert("La cantidad y el costo unitario deben ser mayores a 0.")
    return
  }

  const payload = {
    fechaOrden: form.value.fechaOrden,
    proveedorId: form.value.proveedorId,
    departamentoId: form.value.departamentoId,
    empleadoId: form.value.empleadoId,
    detalles: [
      {
        articuloId: form.value.articuloId,
        cantidad: form.value.cantidad,
        unidadMedidaId: form.value.unidadMedidaId,
        costoUnitario: form.value.costoUnitario
      }
    ]
  }
  await store.addOrden(payload)
  closeModal()
}

const sendAsiento = async (orden) => {
  if (confirm(`¿Marcar la orden ${orden.numero} como recibida? Esto generará el asiento contable.`)) {
    try {
      await store.recibirOrden(orden.numero)
      asientoSuccess.value = true
      orderRecibidaId.value = orden.numero
      setTimeout(() => asientoSuccess.value = false, 5000)
    } catch (e) {
      alert(e.response?.data?.message || "Error al recibir orden.")
    }
  }
}

const ajustarValor = (campo, delta, min = 0.01) => {
  const actual = Number(form.value[campo]) || 0
  form.value[campo] = Math.max(min, actual + delta)
}

const formatCurrency = (val) => {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(val)
}
</script>

<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Órdenes de Compra</h1>
      <button @click="openModal()" class="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg font-medium transition-colors shadow-sm">
        <Plus class="w-5 h-5" /> Nueva Orden
      </button>
    </div>

    <!-- Alert -->
    <div v-if="asientoSuccess" class="mb-6 p-4 bg-emerald-50 border border-emerald-200 rounded-lg flex items-start gap-3">
      <CheckCircle class="w-5 h-5 text-emerald-600 mt-0.5" />
      <div>
        <h4 class="text-emerald-800 font-semibold">Orden Recibida / Asiento Generado</h4>
        <p class="text-emerald-600 text-sm mt-1">La orden No. {{ orderRecibidaId }} ha sido recibida y el asiento contable fue enviado al backend.</p>
      </div>
    </div>

    <!-- Tabs -->
    <div class="flex gap-2 mb-4">
      <button
        @click="activeTab = 'enProceso'"
        :class="[
          'px-4 py-2 rounded-lg font-medium text-sm transition-colors',
          activeTab === 'enProceso' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
        ]">
        En Proceso
      </button>
      <button
        @click="activeTab = 'historial'"
        :class="[
          'px-4 py-2 rounded-lg font-medium text-sm transition-colors',
          activeTab === 'historial' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
        ]">
        Historial (Recibidas / Canceladas)
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
          <input type="text" v-model="searchQuery" placeholder="Buscar órdenes..." class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
        </div>
      </div>
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-gray-50 text-gray-600 text-sm border-b border-gray-100">
            <th class="py-3 px-6 font-semibold">Número</th>
            <th class="py-3 px-6 font-semibold">Fecha</th>
            <th class="py-3 px-6 font-semibold">Proveedor</th>
            <th class="py-3 px-6 font-semibold">Departamento</th>
            <th class="py-3 px-6 font-semibold">Artículo(s)</th>
            <th class="py-3 px-6 font-semibold">Total</th>
            <th class="py-3 px-6 font-semibold text-center">Estado</th>
            <th class="py-3 px-6 font-semibold text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredOrdenes" :key="item.numero" class="border-b border-gray-50 hover:bg-gray-50 transition-colors">
            <td class="py-3 px-6 font-medium text-gray-800">OC-{{ item.numero }}</td>
            <td class="py-3 px-6 text-gray-600">{{ new Date(item.fechaOrden).toLocaleDateString() }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.proveedorNombre }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.departamentoNombre }}</td>
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
            <td class="py-3 px-6 text-right">
              <button v-if="item.estado === 'Pendiente'" @click="aprobar(item)" class="text-blue-600 hover:text-blue-800 mr-3 p-1 rounded-md hover:bg-blue-50 transition-colors inline-block" title="Aprobar Orden">
                <Check class="w-4 h-4" />
              </button>
              <button v-if="item.estado === 'Pendiente'" @click="cancelar(item)" class="text-red-600 hover:text-red-800 mr-3 p-1 rounded-md hover:bg-red-50 transition-colors inline-block" title="Cancelar Orden">
                <Ban class="w-4 h-4" />
              </button>
              <button v-if="item.estado === 'Aprobada'" @click="sendAsiento(item)" class="text-emerald-600 hover:text-emerald-800 mr-3 p-1 rounded-md hover:bg-emerald-50 transition-colors inline-block" title="Marcar como Recibida (Generar Asiento)">
                <Send class="w-4 h-4" />
              </button>
            </td>
          </tr>
          <tr v-if="filteredOrdenes.length === 0"><td colspan="8" class="py-8 text-center text-gray-500">No hay órdenes de compra en esta vista.</td></tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 transition-opacity p-4">
      <div class="bg-white rounded-2xl shadow-xl w-full max-w-2xl overflow-hidden max-h-[90vh] overflow-y-auto">
        <div class="px-6 py-4 border-b border-gray-100 flex justify-between items-center sticky top-0 bg-white z-10">
          <h3 class="text-lg font-bold text-gray-800">Nueva Orden de Compra</h3>
          <button @click="closeModal" class="text-gray-400 hover:text-gray-600"><X class="w-5 h-5" /></button>
        </div>
        <form @submit.prevent="save" class="p-6">
          <div class="grid grid-cols-2 gap-4 mb-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Fecha</label>
              <input v-model="form.fechaOrden" type="date" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Proveedor</label>
              <select v-model="form.proveedorId" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="" disabled>Seleccione proveedor...</option>
                <option v-for="p in proveedores" :key="p.id" :value="p.id">{{ p.nombreComercial }}</option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Departamento</label>
              <select v-model="form.departamentoId" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="" disabled>Seleccione departamento...</option>
                <option v-for="d in departamentos" :key="d.id" :value="d.id">{{ d.nombre }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Empleado</label>
              <select v-model="form.empleadoId" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="" disabled>Seleccione empleado...</option>
                <option v-for="e in empleados" :key="e.id" :value="e.id">{{ e.nombre }}</option>
              </select>
            </div>
            
            <div class="col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Artículo a Comprar</label>
              <select v-model="form.articuloId" @change="onArticuloChange" required class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none">
                <option value="" disabled>Seleccione un artículo...</option>
                <option v-for="a in articulos" :key="a.id" :value="a.id">{{ a.descripcion }} (Disponible: {{ a.existencia }})</option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Cantidad</label>
              <div class="relative">
                <input v-model="form.cantidad" type="number" min="0.01" step="0.01" required class="w-full pl-4 pr-9 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none">
                <div class="absolute right-1 top-1/2 -translate-y-1/2 flex flex-col">
                  <button type="button" tabindex="-1" @click="ajustarValor('cantidad', 1)" class="text-gray-400 hover:text-emerald-600 leading-none"><ChevronUp class="w-4 h-4" /></button>
                  <button type="button" tabindex="-1" @click="ajustarValor('cantidad', -1)" class="text-gray-400 hover:text-emerald-600 leading-none"><ChevronDown class="w-4 h-4" /></button>
                </div>
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Costo Unitario</label>
              <div class="relative">
                <input v-model="form.costoUnitario" type="number" min="0.01" step="0.01" required class="w-full pl-4 pr-9 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 outline-none [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none">
                <div class="absolute right-1 top-1/2 -translate-y-1/2 flex flex-col">
                  <button type="button" tabindex="-1" @click="ajustarValor('costoUnitario', 1)" class="text-gray-400 hover:text-emerald-600 leading-none"><ChevronUp class="w-4 h-4" /></button>
                  <button type="button" tabindex="-1" @click="ajustarValor('costoUnitario', -1)" class="text-gray-400 hover:text-emerald-600 leading-none"><ChevronDown class="w-4 h-4" /></button>
                </div>
              </div>
            </div>
          </div>
          
          <div class="bg-gray-50 p-4 rounded-lg flex justify-between items-center mb-6">
            <span class="text-gray-600">Total de la Orden:</span>
            <span class="text-xl font-bold text-gray-800">{{ formatCurrency(form.cantidad * form.costoUnitario) }}</span>
          </div>

          <div class="flex justify-end gap-3 sticky bottom-0 bg-white py-2">
            <button type="button" @click="closeModal" class="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg font-medium transition-colors">Cancelar</button>
            <button type="submit" class="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-medium transition-colors">Guardar Orden</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
