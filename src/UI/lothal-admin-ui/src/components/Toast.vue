<script setup lang="ts">
import { useProductStore } from '../stores/products';

const store = useProductStore();
</script>

<template>
  <Transition name="toast">
    <div v-if="store.error" class="toast-error" @click="store.error = null">
      <span class="icon">⚠️</span>
      <span class="msg">{{ store.error }}</span>
      <span class="close">×</span>
    </div>
  </Transition>
</template>

<style scoped>
.toast-error {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  background: var(--danger);
  color: white;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  gap: 1rem;
  cursor: pointer;
  z-index: 2000;
  max-width: 400px;
}
.icon { font-size: 1.2rem; }
.msg { flex: 1; font-weight: 500; font-size: 0.9rem; }
.close { font-size: 1.2rem; opacity: 0.7; }

.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { transform: translateX(100px); opacity: 0; }
.toast-leave-to { transform: translateX(100px); opacity: 0; }
</style>
