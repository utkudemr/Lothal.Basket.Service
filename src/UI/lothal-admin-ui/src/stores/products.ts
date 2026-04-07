import { defineStore } from 'pinia';
import { productApi, stockApi } from '../api/gateway';
import type { ProductWithStock, Product } from '../types/product';

export const useProductStore = defineStore('products', {
  state: () => ({
    products: [] as ProductWithStock[],
    totalItems: 0,
    currentPage: 1,
    pageSize: 10,
    loading: false,
    error: null as string | null,
    // Search / autocomplete state
    suggestions: [] as Product[],
    searchQuery: '',
    isSearching: false,
  }),
  actions: {
    async fetchProducts() {
      this.loading = true;
      this.error = null;
      try {
        const from = (this.currentPage - 1) * this.pageSize;
        const response = await productApi.getAll(from, this.pageSize);
        const { data } = response;
        // Handle different casing from backend if necessary
        const items = data.items || [];
        const totalCount = data.totalCount ?? 0;
        
        this.totalItems = totalCount;
        this.products = items.map((p: Product) => ({ ...p, stock: undefined }));
        
        await this.fetchStocksForCurrentProducts();
      } catch (err: any) {
        this.error = "Failed to load products: " + err.message;
      } finally {
        this.loading = false;
      }
    },

    setPage(page: number) {
      if (page < 1 || (this.totalItems > 0 && page > Math.ceil(this.totalItems / this.pageSize))) return;
      this.currentPage = page;
      this.fetchProducts();
    },

    async fetchStocksForCurrentProducts() {
      const barcodes = this.products.map((p: ProductWithStock) => p.barcode);
      if (barcodes.length === 0) return;

      try {
        const res = await stockApi.getBatch(barcodes);
        const stockMap = new Map(res.data.map(s => [s.barcode, s]));
        this.products.forEach((p: ProductWithStock) => {
          p.stock = stockMap.get(p.barcode);
        });
      } catch (e) {
        console.warn('Could not fetch batch stocks', e);
      }
    },

    async upsertProduct(product: Product) {
      this.loading = true;
      try {
        await productApi.upsert([product]);
        await this.fetchProducts(); // Refresh list
      } catch (err: any) {
        this.error = "Failed to save product: " + err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async deleteProduct(barcode: string) {
      try {
        await productApi.delete(barcode);
        await this.fetchProducts(); // Refresh current page
      } catch (err: any) {
        this.error = "Failed to delete product: " + err.message;
        throw err;
      }
    },

    async updateStock(barcode: string, quantity: number, mode: 'upsert' | 'reserve' | 'release') {
      try {
        if (mode === 'upsert') {
          await stockApi.upsert(barcode, quantity);
        } else if (mode === 'reserve') {
          await stockApi.reserve(barcode, quantity);
        } else if (mode === 'release') {
          await stockApi.release(barcode, quantity);
        }
        
        // Find and update local stock
        const product = this.products.find(p => p.barcode === barcode);
        if (product) {
          const res = await stockApi.getByBarcode(barcode);
          product.stock = res.data;
        }
      } catch (err: any) {
        this.error = `Failed to update stock (${mode}): ` + (err.response?.data?.reason || err.message);
        throw err;
      }
    },

    async bulkIncreaseStock(items: Array<{ barcode: string; amount: number }>) {
      this.loading = true;
      try {
        await stockApi.bulkAdjustStocks(items);
        await this.fetchProducts(); // Refresh current page to see changes
      } catch (err: any) {
        this.error = "Bulk stock update failed: " + err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async searchProducts(q: string, signal?: AbortSignal) {
      if (!q.trim()) {
        this.suggestions = [];
        this.searchQuery = '';
        return;
      }
      this.searchQuery = q;
      this.isSearching = true;
      try {
        const res = await productApi.search(q, 10, signal);
        this.suggestions = res.data;
      } catch (err: any) {
        // AbortError = kullanıcı hızlı yazdı, yeni istek açıldı → ignore
        if (err.name === 'CanceledError' || err.code === 'ERR_CANCELED') return;
        console.warn('[store] search error', err.message);
      } finally {
        this.isSearching = false;
      }
    },

    clearSearch() {
      this.suggestions = [];
      this.searchQuery = '';
    },
  }
});
