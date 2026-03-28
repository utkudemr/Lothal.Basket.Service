<script setup lang="ts">
import { useProductStore } from '../stores/products';

const store = useProductStore();

const emit = defineEmits(['edit-product', 'adjust-stock']);

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
</script>

<template>
  <div class="table-container glass-card">
    <table>
      <thead>
        <tr>
          <th>Barcode</th>
          <th>Name</th>
          <th>Class/Color/Size</th>
          <th>Price</th>
          <th>Stock (WS/Avail)</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="product in store.products" :key="product.barcode" class="animate-fade-in">
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
    <div v-if="store.loading" class="loading-overlay">
       <div class="spinner"></div>
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
</style>
