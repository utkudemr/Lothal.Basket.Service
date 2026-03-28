import { defineStore } from 'pinia';
import { productApi, stockApi } from '../api/gateway';
export const useProductStore = defineStore('products', {
    state: () => ({
        products: [],
        loading: false,
        error: null,
    }),
    actions: {
        async fetchProducts() {
            this.loading = true;
            this.error = null;
            try {
                const response = await productApi.getAll();
                const baseProducts = response.data;
                // Map to include stock as undefined initially
                this.products = baseProducts.map(p => ({ ...p, stock: undefined }));
                await this.fetchStocksForCurrentProducts();
            }
            catch (err) {
                this.error = "Failed to load products: " + err.message;
            }
            finally {
                this.loading = false;
            }
        },
        async fetchStocksForCurrentProducts() {
            const stockPromises = this.products.map(async (p) => {
                try {
                    const res = await stockApi.getByBarcode(p.barcode);
                    p.stock = res.data;
                }
                catch (e) {
                    console.warn(`Could not fetch stock for ${p.barcode}`, e);
                }
            });
            await Promise.allSettled(stockPromises);
        },
        async upsertProduct(product) {
            this.loading = true;
            try {
                await productApi.upsert([product]);
                await this.fetchProducts(); // Refresh list
            }
            catch (err) {
                this.error = "Failed to save product: " + err.message;
                throw err;
            }
            finally {
                this.loading = false;
            }
        },
        async deleteProduct(barcode) {
            try {
                await productApi.delete(barcode);
                this.products = this.products.filter(p => p.barcode !== barcode);
            }
            catch (err) {
                this.error = "Failed to delete product: " + err.message;
                throw err;
            }
        },
        async updateStock(barcode, quantity, mode) {
            try {
                if (mode === 'upsert') {
                    await stockApi.upsert(barcode, quantity);
                }
                else if (mode === 'reserve') {
                    await stockApi.reserve(barcode, quantity);
                }
                else if (mode === 'release') {
                    await stockApi.release(barcode, quantity);
                }
                // Find and update local stock
                const product = this.products.find(p => p.barcode === barcode);
                if (product) {
                    const res = await stockApi.getByBarcode(barcode);
                    product.stock = res.data;
                }
            }
            catch (err) {
                this.error = `Failed to update stock (${mode}): ` + (err.response?.data?.reason || err.message);
                throw err;
            }
        }
    }
});
//# sourceMappingURL=products.js.map