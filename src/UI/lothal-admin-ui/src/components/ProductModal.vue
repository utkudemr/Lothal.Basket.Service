<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Product } from '../types/product';

const props = defineProps<{
  show: boolean;
  product?: Product;
}>();

const emit = defineEmits(['close', 'save']);

const formData = ref<Product>({
  barcode: '',
  name: '',
  class: 'Standard',
  color: 'Common',
  size: 'M',
  price: 0
});

watch(() => props.product, (newVal) => {
  if (newVal) {
    formData.value = { ...newVal };
  } else {
    formData.value = { barcode: '', name: '', class: 'Standard', color: 'Common', size: 'M', price: 0 };
  }
}, { immediate: true });

const submit = () => {
  emit('save', { ...formData.value });
};
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content glass-card animate-fade-in">
      <h3>{{ product ? 'Edit Product' : 'Add New Product' }}</h3>
      <form @submit.prevent="submit">
        <div class="form-group">
          <label>Barcode</label>
          <input v-model="formData.barcode" required :disabled="!!product" placeholder="Unique Barcode">
        </div>
        <div class="form-group">
          <label>Name</label>
          <input v-model="formData.name" required placeholder="Product Name">
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Price ($)</label>
            <input v-model.number="formData.price" type="number" step="0.01" required>
          </div>
          <div class="form-group">
            <label>Class</label>
            <input v-model="formData.class">
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Color</label>
            <input v-model="formData.color">
          </div>
          <div class="form-group">
            <label>Size</label>
            <input v-model="formData.size">
          </div>
        </div>
        
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="emit('close')">Cancel</button>
          <button type="submit" class="btn btn-primary">{{ product ? 'Update' : 'Create' }}</button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.7);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.modal-content {
  width: 90%;
  max-width: 500px;
}
.form-group {
  margin-bottom: 1.2rem;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}
label {
  font-size: 0.9rem;
  color: var(--text-dim);
}
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}
</style>
