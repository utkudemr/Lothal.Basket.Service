<script setup lang="ts">
import { ref, watch } from 'vue';
import type { ProductWithStock } from '../types/product';

const props = defineProps<{
  show: boolean;
  loading?: boolean;
  selectedProducts: ProductWithStock[];
}>();

const emit = defineEmits(['close', 'confirm']);

const adjustments = ref<Record<string, number>>({});

// Initialize/Sync adjustments when modal opens or selection changes
watch(() => props.selectedProducts, (newProducts) => {
  newProducts.forEach(p => {
    if (adjustments.value[p.barcode] === undefined) {
      adjustments.value[p.barcode] = 1000; // Default increment
    }
  });
}, { immediate: true });

const handleConfirm = () => {
  const items = props.selectedProducts.map(p => ({
    barcode: p.barcode,
    amount: adjustments.value[p.barcode] || 0
  }));
  emit('confirm', items);
};
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content glass-card animate-fade-in">
      <h3>Bulk Stock Increase 📦</h3>
      <p class="text-dim mb-1">Set the stock increment for each selected product.</p>

      <div class="product-adjust-list">
        <div v-for="product in selectedProducts" :key="product.barcode" class="adjust-item">
          <div class="item-info">
            <span class="barcode">{{ product.barcode }}</span>
            <span class="name">{{ product.name }}</span>
          </div>
          <div class="item-input">
            <input 
              type="number" 
              v-model.number="adjustments[product.barcode]" 
              class="amount-input"
              placeholder="Qty"
            />
          </div>
        </div>
        <div v-if="selectedProducts.length === 0" class="empty-state">
          No products selected. Close this and select items first.
        </div>
      </div>

      <div class="form-actions mt-2">
        <button class="btn btn-secondary" @click="emit('close')" :disabled="loading">Cancel</button>
        <button class="btn btn-primary btn-glow" :disabled="loading || selectedProducts.length === 0" @click="handleConfirm">
          {{ loading ? 'Updating...' : `Update ${selectedProducts.length} Products` }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.7);
  backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center;
  z-index: 1001;
}
.modal-content { 
  width: 95%; 
  max-width: 500px; 
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  border-color: var(--accent-color);
  box-shadow: 0 0 20px rgba(124, 58, 237, 0.2);
}
.product-adjust-list {
  margin: 1rem 0;
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 8px;
  background: rgba(0,0,0,0.2);
}
.adjust-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid rgba(255,255,255,0.05);
}
.adjust-item:last-child { border-bottom: none; }
.item-info {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}
.barcode { font-family: monospace; font-size: 0.8rem; color: var(--accent-color); }
.name { font-size: 0.9rem; font-weight: 500; }
.amount-input {
  width: 80px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.2);
  color: white;
  padding: 0.4rem;
  border-radius: 4px;
  text-align: right;
}
.empty-state { padding: 2rem; text-align: center; color: var(--text-dim); }
.mb-1 { margin-bottom: 0.5rem; }
.mt-2 { margin-top: 1.5rem; }
.form-actions { 
  display: flex; 
  justify-content: flex-end; 
  gap: 1rem; 
}
.btn-glow {
  box-shadow: 0 0 10px rgba(124, 58, 237, 0.4);
}
.btn-glow:hover {
  box-shadow: 0 0 20px rgba(124, 58, 237, 0.8);
}
</style>
