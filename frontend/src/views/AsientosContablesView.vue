<script setup>
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useAsientosContablesStore } from '../stores/asientosContablesStore'
import { RefreshCw } from '@lucide/vue'

const store = useAsientosContablesStore()
const { asientos, isLoading } = storeToRefs(store)

onMounted(() => {
  store.fetchAsientos()
})

const reenviar = async (id) => {
  if (confirm('¿Está seguro de que desea reenviar este asiento contable?')) {
    await store.reenviarAsiento(id)
  }
}

const formatCurrency = (val) => {
  return new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(val)
}
</script>

<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Asientos Contables</h1>
      <button @click="store.fetchAsientos()" class="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg font-medium transition-colors shadow-sm">
        <RefreshCw class="w-4 h-4" /> Refrescar
      </button>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="text-center py-10">
      <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-emerald-600 mx-auto"></div>
    </div>

    <!-- Table -->
    <div v-else class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <table class="w-full text-left border-collapse min-w-[800px]">
        <thead>
          <tr class="bg-gray-50 text-gray-600 text-sm border-b border-gray-100">
            <th class="py-3 px-6 font-semibold">ID</th>
            <th class="py-3 px-6 font-semibold">No. Asiento</th>
            <th class="py-3 px-6 font-semibold">Descripción</th>
            <th class="py-3 px-6 font-semibold">Cuenta Débito</th>
            <th class="py-3 px-6 font-semibold">Cuenta Crédito</th>
            <th class="py-3 px-6 font-semibold">Monto</th>
            <th class="py-3 px-6 font-semibold">OC No.</th>
            <th class="py-3 px-6 font-semibold text-center">Estado</th>
            <th class="py-3 px-6 font-semibold text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in asientos" :key="item.id" class="border-b border-gray-50 hover:bg-gray-50 transition-colors">
            <td class="py-3 px-6 font-medium text-gray-800">{{ item.id }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.asiento ?? '—' }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.descripcion }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.cuentaDebitoId }}</td>
            <td class="py-3 px-6 text-gray-600">{{ item.cuentaCreditoId }}</td>
            <td class="py-3 px-6 text-gray-600 font-medium">{{ formatCurrency(item.montoAsiento) }}</td>
            <td class="py-3 px-6 text-gray-600">
              <span v-if="item.ordenCompraNumero">OC-{{ item.ordenCompraNumero }}</span>
              <span v-else class="text-gray-400">N/A</span>
            </td>
            <td class="py-3 px-6 text-center">
              <span :class="[
                'px-3 py-1 rounded-full text-xs font-medium',
                item.estado === 'Enviado' ? 'bg-emerald-100 text-emerald-700' : 
                item.estado === 'Error' ? 'bg-red-100 text-red-700' : 'bg-amber-100 text-amber-700'
              ]" :title="item.mensajeError">
                {{ item.estado }}
              </span>
            </td>
            <td class="py-3 px-6 text-right">
              <button v-if="item.estado === 'Error' || item.estado === 'Pendiente'" @click="reenviar(item.id)" class="text-blue-600 hover:text-blue-800 mr-3 p-1 rounded-md hover:bg-blue-50 transition-colors inline-block" title="Reenviar a Contabilidad">
                <RefreshCw class="w-4 h-4" />
              </button>
            </td>
          </tr>
          <tr v-if="asientos.length === 0"><td colspan="9" class="py-8 text-center text-gray-500">No hay asientos contables registrados.</td></tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
