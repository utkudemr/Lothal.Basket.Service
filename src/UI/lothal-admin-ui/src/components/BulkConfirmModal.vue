<script setup lang="ts">


const props = defineProps<{
  show: boolean;
  loading?: boolean;
}>();

const emit = defineEmits(['close', 'confirm']);
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content glass-card animate-fade-in">
      <h3>Bulk Stock Increase 📦</h3>
      <p class="warning-text">
        Are you sure you want to add 1000 stock to <strong>ALL</strong> products in the system? 
        This simulates a high-traffic warehouse update and syncs across the entire infrastructure.
      </p>

      <div class="form-actions mt-2">
        <button class="btn btn-secondary" @click="emit('close')" :disabled="loading">Cancel</button>
        <button class="btn btn-primary btn-glow" :disabled="loading" @click="emit('confirm')">
          {{ loading ? 'Broadcasting...' : 'Yes, Inject 1000 Stock' }}
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
  width: 90%; 
  max-width: 450px; 
  border-color: var(--accent-color);
  box-shadow: 0 0 20px rgba(124, 58, 237, 0.2);
}
.warning-text { 
  font-size: 0.95rem; 
  color: var(--text-main); 
  margin: 1.5rem 0; 
  line-height: 1.5;
}
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
