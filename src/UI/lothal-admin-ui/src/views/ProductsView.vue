<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useProductStore } from '../stores/products';
import ProductTable from '../components/ProductTable.vue';
import ProductModal from '../components/ProductModal.vue';
import StockAdjustModal from '../components/StockAdjustModal.vue';
import type { Product } from '../types/product';

const store = useProductStore();

const showProductModal = ref(false);
const showStockModal = ref(false);
const selectedProduct = ref<Product | null>(null);

onMounted(() => {
  store.fetchProducts();
});

const openCreate = () => {
  selectedProduct.value = null;
  showProductModal.value = true;
};

const openEdit = (product: Product) => {
  selectedProduct.value = product;
  showProductModal.value = true;
};

const openStockAdjust = (product: Product) => {
  selectedProduct.value = product;
  showStockModal.value = true;
};

const handleSaveProduct = async (data: Product) => {
  try {
    await store.upsertProduct(data);
    showProductModal.value = false;
  } catch (err) {
    // Error handled by store/toast
  }
};
</script>

<template>
  <main class="container">
    <header class="page-header animate-fade-in">
      <div>
        <h1>Stock & Product Management</h1>
        <p class="text-dim">Manage your inventory across the Lothal core services.</p>
      </div>
      <button class="btn btn-primary" @click="openCreate">
        <span>+</span> Add New Product
      </button>
    </header>

    <div class="stats-overview animate-fade-in" style="animation-delay: 0.1s">
      <div class="glass-card stat">
        <label>Total Products</label>
        <div class="val">{{ store.products.length }}</div>
      </div>
      <div class="glass-card stat">
        <label>Low Stock</label>
        <div class="val">{{ store.products.filter(p => (p.stock?.warehouseQuantity ?? 0) < 10).length }}</div>
      </div>
    </div>

    <ProductTable 
      @edit-product="openEdit" 
      @adjust-stock="openStockAdjust" 
      style="animation-delay: 0.2s"
    />

    <!-- Modals -->
    <ProductModal 
      :show="showProductModal" 
      :product="selectedProduct || undefined" 
      @close="showProductModal = false"
      @save="handleSaveProduct"
    />

    <StockAdjustModal 
      :show="showStockModal" 
      :product="selectedProduct"
      @close="showStockModal = false"
    />
  </main>
</template>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}
.page-header p {
  margin-top: 0.3rem;
}
.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}
.stat label {
  font-size: 0.8rem;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 1px;
}
.stat .val {
  font-size: 2.5rem;
  font-weight: 700;
  color: var(--accent-color);
}
</style>
