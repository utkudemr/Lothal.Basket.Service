<script setup lang="ts">
import { ref } from 'vue';
import type { Product } from '../types/product';
import { useProductStore } from '../stores/products';

const props = defineProps<{
  show: boolean;
  product: Product | null;
}>();

const emit = defineEmits(['close']);
const store = useProductStore();

const amount = ref(1);
const mode = ref<'upsert' | 'reserve' | 'release'>('upsert');
const submitting = ref(false);

const handleAdjust = async () => {
  if (!props.product) return;
  submitting.value = true;
  try {
    await store.updateStock(props.product.barcode, amount.value, mode.value);
    emit('close');
  } finally {
    submitting.value = false;
  }
};
</script>

<template>
  <div v-if="show && product" class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content glass-card animate-fade-in">
      <h3>Adjust Stock: {{ product.name }}</h3>
      <p class="barcode-sub">Barcode: {{ product.barcode }}</p>

      <div class="mode-selector">
        <button 
          class="btn" 
          :class="mode === 'upsert' ? 'btn-primary' : 'btn-secondary'"
          @click="mode = 'upsert'"
        >Set Absolute</button>
        <button 
          class="btn" 
          :class="mode === 'reserve' ? 'btn-primary' : 'btn-secondary'"
          @click="mode = 'reserve'"
        >Reserve (-)</button>
        <button 
          class="btn" 
          :class="mode === 'release' ? 'btn-primary' : 'btn-secondary'"
          @click="mode = 'release'"
        >Release (+)</button>
      </div>

      <div class="form-group mt-2">
        <label>Quantity</label>
        <input v-model.number="amount" type="number" min="1" required>
      </div>

      <p v-if="mode === 'upsert'" class="hint">This will set the TOTAL warehouse quantity to {{ amount }}.</p>
      <p v-if="mode === 'reserve'" class="hint">This will decrease available stock by {{ amount }}.</p>
      <p v-if="mode === 'release'" class="hint">This will increase available stock by {{ amount }}.</p>

      <div class="form-actions">
        <button class="btn btn-secondary" @click="emit('close')">Cancel</button>
        <button class="btn btn-primary" :disabled="submitting" @click="handleAdjust">
          {{ submitting ? 'Processing...' : 'Apply Changes' }}
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
.modal-content { width: 90%; max-width: 400px; }
.barcode-sub { font-size: 0.8rem; color: var(--text-dim); margin-bottom: 1.5rem; }
.mode-selector { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.5rem; margin-bottom: 1.5rem; }
.mode-selector .btn { padding: 0.5rem; font-size: 0.8rem; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1rem; }
.hint { font-size: 0.85rem; color: var(--accent-color); font-style: italic; }
.mt-2 { margin-top: 1rem; }
.form-actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 2rem; }
</style>
