<script setup lang="ts">
import { useProductStore } from '../stores/products';
import { computed } from 'vue';

const store = useProductStore();

const props = defineProps<{
  selectedBarcodes: string[];
}>();

const emit = defineEmits(['edit-product', 'adjust-stock', 'update:selectedBarcodes']);

const isSelected = (barcode: string) => props.selectedBarcodes.includes(barcode);

const toggleSelection = (barcode: string) => {
  const newSelection = isSelected(barcode)
    ? props.selectedBarcodes.filter(id => id !== barcode)
    : [...props.selectedBarcodes, barcode];
  emit('update:selectedBarcodes', newSelection);
};

const toggleSelectAll = () => {
  if (store.products.length > 0 && props.selectedBarcodes.length === store.products.length) {
    emit('update:selectedBarcodes', []);
  } else {
    emit('update:selectedBarcodes', store.products.map(p => p.barcode));
  }
};

const deleteProduct = async (barcode: string) => {
  if (confirm('Are you sure you want to delete this product?')) {
    await store.deleteProduct(barcode);
  }
};

const getStockBadgeClass = (qty: number) => {
  if (qty <= 0) return 'badge-danger';
  if (qty < 10) return 'badge-warning';
  return 'badge-success';
};

const totalPages = computed(() => Math.ceil(store.totalItems / store.pageSize));
const startItem = computed(() => (store.currentPage - 1) * store.pageSize + 1);
const endItem = computed(() => Math.min(store.currentPage * store.pageSize, store.totalItems));
</script>

<template>
  <div class="table-container glass-card">
    <table>
      <thead>
        <tr>
          <th style="width: 40px;">
            <input 
              type="checkbox" 
              :checked="store.products.length > 0 && selectedBarcodes.length === store.products.length"
              @change="toggleSelectAll"
            />
          </th>
          <th>Barcode</th>
          <th>Name</th>
          <th>Class/Color/Size</th>
          <th>Price</th>
          <th>Stock (WS/Avail)</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="product in store.products" :key="product.barcode" class="animate-fade-in" :class="{ 'row-selected': isSelected(product.barcode) }">
          <td>
            <input 
              type="checkbox" 
              :checked="isSelected(product.barcode)"
              @change="toggleSelection(product.barcode)"
            />
          </td>
          <td><code>{{ product.barcode }}</code></td>
          <td><strong>{{ product.name }}</strong></td>
          <td>{{ product.class }} / {{ product.color }} / {{ product.size }}</td>
          <td>${{ product.price.toFixed(2) }}</td>
          <td>
            <div v-if="product.stock" class="stock-info">
              <span class="badge" :class="getStockBadgeClass(product.stock.warehouseQuantity)">
                {{ product.stock.warehouseQuantity }}
              </span>
              /
              <span>{{ product.stock.availableQuantity ?? '--' }}</span>
            </div>
            <div v-else class="text-dim">No stock data</div>
          </td>
          <td>
            <div class="actions">
              <button class="btn btn-secondary btn-icon" title="Adjust Stock" @click="emit('adjust-stock', product)">
                📦
              </button>
              <button class="btn btn-secondary btn-icon" title="Edit Product" @click="emit('edit-product', product)">
                ✏️
              </button>
              <button class="btn btn-danger btn-icon" title="Delete Product" @click="deleteProduct(product.barcode)">
                🗑️
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    
    <div v-if="store.loading && store.products.length === 0" class="loading-overlay">
       <div class="spinner"></div>
    </div>

    <!-- Pagination Footer -->
    <div class="pagination-footer">
      <div class="pagination-info">
        Showing {{ startItem }} - {{ endItem }} of {{ store.totalItems }} products
      </div>
      <div class="pagination-controls">
        <button 
          class="btn btn-secondary" 
          :disabled="store.currentPage <= 1 || store.loading"
          @click="store.setPage(store.currentPage - 1)"
        >
          Previous
        </button>
        <span class="page-number">Page {{ store.currentPage }} of {{ totalPages }}</span>
        <button 
          class="btn btn-secondary" 
          :disabled="store.currentPage >= totalPages || store.loading"
          @click="store.setPage(store.currentPage + 1)"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.actions {
  display: flex;
  gap: 0.5rem;
}
.stock-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.loading-overlay {
  padding: 2rem;
  text-align: center;
}
.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid rgba(255,255,255,0.1);
  border-top-color: var(--accent-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}

.pagination-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.02);
}
.pagination-info {
  font-size: 0.9rem;
  color: var(--text-dim);
}
.pagination-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.page-number {
  font-weight: 500;
  min-width: 100px;
  text-align: center;
}
.row-selected {
  background: rgba(124, 58, 237, 0.05);
}
</style>
