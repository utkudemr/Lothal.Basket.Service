<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import { useProductStore } from '../stores/products';
import ProductTable from '../components/ProductTable.vue';
import ProductModal from '../components/ProductModal.vue';
import StockAdjustModal from '../components/StockAdjustModal.vue';
import BulkConfirmModal from '../components/BulkConfirmModal.vue';
import type { Product } from '../types/product';

const store = useProductStore();

const showProductModal = ref(false);
const showStockModal = ref(false);
const showBulkModal = ref(false);
const selectedProduct = ref<Product | null>(null);
const selectedBarcodes = ref<string[]>([]);

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

const bulkAddStockPrompt = () => {
  if (selectedBarcodes.value.length === 0) {
    alert('Please select at least one product to increase stock.');
    return;
  }
  showBulkModal.value = true;
};

const selectedProductsForBulk = computed(() => 
  store.products.filter(p => selectedBarcodes.value.includes(p.barcode))
);

const executeBulkStock = async (items: Array<{ barcode: string, amount: number }>) => {
  console.log('[UI] User confirmed bulk stock update for', items.length, 'items');
  try {
    await store.bulkIncreaseStock(items);
    showBulkModal.value = false;
    selectedBarcodes.value = []; // Clear selection after success
    alert('Bulk stock adjustment applied successfully!');
  } catch (err: any) {
    console.error('[UI] Bulk stock update failed', err);
  }
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
      <div class="header-actions">
        <button class="btn btn-secondary" @click="bulkAddStockPrompt" :disabled="store.loading">
          📦 Bulk Increase ({{ selectedBarcodes.length }})
        </button>
        <button class="btn btn-primary" @click="openCreate">
          <span>+</span> Add New Product
        </button>
      </div>
    </header>

    <div class="stats-overview animate-fade-in" style="animation-delay: 0.1s">
      <div class="glass-card stat">
        <label>Total Products</label>
        <div class="val">{{ store.totalItems }}</div>
      </div>
      <div class="glass-card stat">
        <label>Low Stock</label>
        <div class="val">{{ store.products.filter(p => (p.stock?.warehouseQuantity ?? 0) < 10).length }} (Page)</div>
      </div>
    </div>

    <ProductTable 
      v-model:selectedBarcodes="selectedBarcodes"
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

    <BulkConfirmModal 
      :show="showBulkModal" 
      :loading="store.loading"
      :selectedProducts="selectedProductsForBulk"
      @close="showBulkModal = false"
      @confirm="executeBulkStock" 
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
