import axios from 'axios';
const gatewayClient = axios.create({
    baseURL: 'http://localhost:5024', // API Gateway URL
    headers: {
        'Content-Type': 'application/json',
    },
});
export const productApi = {
    getAll: (from = 0, size = 100) => gatewayClient.get(`/api/gateway/products?from=${from}&size=${size}`),
    getByBarcode: (barcode) => gatewayClient.get(`/api/gateway/products/${barcode}`),
    upsert: (products) => gatewayClient.post('/api/gateway/products/bulk-merge', { products }),
    delete: (barcode) => gatewayClient.delete(`/api/gateway/products/${barcode}`),
};
export const stockApi = {
    getByBarcode: (barcode) => gatewayClient.get(`/api/stocks/${barcode}`),
    upsert: (barcode, quantity, source = 'MANUAL') => gatewayClient.put('/api/stocks/upsert', { barcode, warehouseQuantity: quantity, source }),
    reserve: (barcode, quantity) => gatewayClient.post(`/api/stocks/${barcode}/reserve`, { quantity }),
    release: (barcode, quantity) => gatewayClient.post(`/api/stocks/${barcode}/release`, { quantity }),
};
//# sourceMappingURL=gateway.js.map