<script setup lang="ts">
import { ref, onUnmounted } from 'vue';
import { useProductStore } from '../stores/products';

const emit = defineEmits<{
  (e: 'select', barcode: string): void;
}>();

const store = useProductStore();
const inputRef = ref<HTMLInputElement | null>(null);
const containerRef = ref<HTMLElement | null>(null);
const isOpen = ref(false);
const activeIndex = ref(-1);

// Dropdown'ın ekranda nereye konumlanacağını tutar (fixed positioning için)
const dropdownStyle = ref({ top: '0px', left: '0px', width: '0px' });

// Debounce + AbortController
let debounceTimer: ReturnType<typeof setTimeout> | null = null;
let abortController: AbortController | null = null;

const DEBOUNCE_MS = 300;

/** Search container'ının konumunu hesaplar ve dropdown style'ını günceller */
function updateDropdownPosition() {
  if (!containerRef.value) return;
  const rect = containerRef.value.getBoundingClientRect();
  dropdownStyle.value = {
    top: `${rect.bottom + 8}px`,
    left: `${rect.left}px`,
    width: `${rect.width}px`,
  };
}

function onInput(event: Event) {
  const q = (event.target as HTMLInputElement).value;
  activeIndex.value = -1;

  if (debounceTimer) clearTimeout(debounceTimer);
  if (abortController) abortController.abort();

  if (!q.trim()) {
    store.clearSearch();
    isOpen.value = false;
    return;
  }

  debounceTimer = setTimeout(async () => {
    abortController = new AbortController();
    await store.searchProducts(q, abortController.signal);
    isOpen.value = store.suggestions.length > 0 || q.trim().length > 0;
    if (isOpen.value) updateDropdownPosition();
  }, DEBOUNCE_MS);
}

function selectSuggestion(barcode: string) {
  isOpen.value = false;
  store.clearSearch();
  if (inputRef.value) inputRef.value.value = '';
  emit('select', barcode);
}

function onKeydown(e: KeyboardEvent) {
  if (!isOpen.value) return;

  if (e.key === 'ArrowDown') {
    e.preventDefault();
    activeIndex.value = Math.min(activeIndex.value + 1, store.suggestions.length - 1);
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    activeIndex.value = Math.max(activeIndex.value - 1, 0);
  } else if (e.key === 'Enter' && activeIndex.value >= 0) {
    e.preventDefault();
    selectSuggestion(store.suggestions[activeIndex.value].barcode);
  } else if (e.key === 'Escape') {
    isOpen.value = false;
    store.clearSearch();
  }
}

function onFocus() {
  if (store.suggestions.length > 0) {
    isOpen.value = true;
    updateDropdownPosition();
  }
}

function onBlur() {
  // Küçük gecikme ile kapat — tıklama olayının önüne geçme
  setTimeout(() => { isOpen.value = false; }, 150);
}

onUnmounted(() => {
  if (debounceTimer) clearTimeout(debounceTimer);
  if (abortController) abortController.abort();
});
</script>

<template>
  <div class="search-wrapper">
    <div ref="containerRef" class="search-container" :class="{ 'search-active': isOpen }">
      <div class="search-icon">
        <svg v-if="!store.isSearching" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
          fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8" /><path d="m21 21-4.3-4.3" />
        </svg>
        <div v-else class="spinner" />
      </div>

      <input
        id="product-search-input"
        ref="inputRef"
        type="text"
        class="search-input"
        placeholder="Ürün adı ile ara... (örn. kırmızı)"
        autocomplete="off"
        @input="onInput"
        @keydown="onKeydown"
        @blur="onBlur"
        @focus="onFocus"
      />

      <kbd class="search-shortcut" aria-hidden="true">ESC</kbd>
    </div>

    <!-- Dropdown: <body>'ye teleport edildi — her türlü overflow/z-index context'ini aşar -->
    <Teleport to="body">
      <Transition name="dropdown">
        <div
          v-if="isOpen"
          class="suggestions-dropdown"
          role="listbox"
          aria-label="Ürün önerileri"
          :style="dropdownStyle"
        >
          <!-- Sonuç var -->
          <template v-if="store.suggestions.length > 0">
            <button
              v-for="(product, idx) in store.suggestions"
              :key="product.barcode"
              class="suggestion-item"
              :class="{ 'suggestion-active': idx === activeIndex }"
              role="option"
              :aria-selected="idx === activeIndex"
              @mousedown.prevent="selectSuggestion(product.barcode)"
            >
              <div class="suggestion-name">{{ product.name }}</div>
              <div class="suggestion-meta">
                <span class="suggestion-barcode">{{ product.barcode }}</span>
                <span class="suggestion-price">₺{{ product.price.toFixed(2) }}</span>
              </div>
            </button>
          </template>

          <!-- Sonuç yok -->
          <div v-else-if="!store.isSearching && store.searchQuery" class="suggestion-empty">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"
              fill="none" stroke="currentColor" stroke-width="1.5">
              <circle cx="11" cy="11" r="8" /><path d="m21 21-4.3-4.3" /><path d="M8 11h6" />
            </svg>
            <span>"<strong>{{ store.searchQuery }}</strong>" için ürün bulunamadı.</span>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.search-wrapper {
  position: relative;
  width: 100%;
  max-width: 680px;
  margin: 0 auto 2rem;
}

.search-container {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--surface-2, rgba(255, 255, 255, 0.06));
  border: 1.5px solid rgba(255, 255, 255, 0.1);
  border-radius: 14px;
  padding: 0.75rem 1.1rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
  backdrop-filter: blur(12px);
}

.search-container:focus-within,
.search-container.search-active {
  border-color: var(--accent-color, #6366f1);
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.18);
  background: var(--surface-3, rgba(255, 255, 255, 0.09));
}

.search-icon {
  color: var(--text-dim, #9ca3af);
  flex-shrink: 0;
  display: flex;
  align-items: center;
}

.search-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  color: var(--text-primary, #f1f5f9);
  font-size: 1rem;
  font-family: inherit;
}

.search-input::placeholder {
  color: var(--text-dim, #6b7280);
}

.search-shortcut {
  font-size: 0.7rem;
  color: var(--text-dim, #6b7280);
  background: rgba(255,255,255,0.07);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 5px;
  padding: 2px 6px;
  font-family: monospace;
  flex-shrink: 0;
}

/* Dropdown — body'ye teleport edildiği için fixed positioning kullanılır */
.suggestions-dropdown {
  position: fixed;
  background: var(--surface-dropdown, #1e1e2e);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  overflow: hidden;
  z-index: 99999;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(16px);
}

.suggestion-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  padding: 0.75rem 1.1rem;
  background: transparent;
  border: none;
  text-align: left;
  cursor: pointer;
  transition: background 0.12s ease;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.suggestion-item:last-child {
  border-bottom: none;
}

.suggestion-item:hover,
.suggestion-active {
  background: rgba(99, 102, 241, 0.15);
}

.suggestion-name {
  font-size: 0.9rem;
  color: var(--text-primary, #f1f5f9);
  font-weight: 500;
}

.suggestion-meta {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.suggestion-barcode {
  font-size: 0.75rem;
  color: var(--text-dim, #6b7280);
  font-family: monospace;
}

.suggestion-price {
  font-size: 0.8rem;
  color: var(--accent-color, #6366f1);
  font-weight: 600;
}

.suggestion-empty {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1rem 1.1rem;
  color: var(--text-dim, #6b7280);
  font-size: 0.87rem;
}

.suggestion-empty svg {
  flex-shrink: 0;
}

/* Spinner */
.spinner {
  width: 18px;
  height: 18px;
  border: 2.5px solid rgba(255,255,255,0.15);
  border-top-color: var(--accent-color, #6366f1);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Dropdown transition */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
