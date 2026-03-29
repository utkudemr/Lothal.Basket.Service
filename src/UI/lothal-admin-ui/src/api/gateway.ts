import axios from 'axios';
import type { Product, Stock } from '../types/product';

const gatewayClient = axios.create({
  baseURL: 'http://localhost:5024', // API Gateway URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export const productApi = {
  getAll: (from = 0, size = 100) => 
    gatewayClient.get<PagedResult<Product>>(`/api/gateway/products?from=${from}&size=${size}`),
  
  getByBarcode: (barcode: string) => 
    gatewayClient.get<Product>(`/api/gateway/products/${barcode}`),
  
  upsert: (products: Product[]) => 
    gatewayClient.post('/api/gateway/products/bulk-merge', { products }),
  
  delete: (barcode: string) => 
    gatewayClient.delete(`/api/gateway/products/${barcode}`),
};

export const stockApi = {
  getByBarcode: (barcode: string) => 
    gatewayClient.get<Stock>(`/api/stocks/${barcode}`),
  
  upsert: (barcode: string, quantity: number, source: string = 'MANUAL') => 
    gatewayClient.put('/api/stocks/upsert', { barcode, warehouseQuantity: quantity, source }),
  
  reserve: (barcode: string, quantity: number) => 
    gatewayClient.post(`/api/stocks/${barcode}/reserve`, { quantity }),
  
  release: (barcode: string, quantity: number) => {
    const transactionId = (window.crypto && window.crypto.randomUUID) 
      ? window.crypto.randomUUID() 
      : Math.random().toString(36).substring(2) + Date.now().toString(36);
    return gatewayClient.post(`/api/stocks/release`, { barcode, quantity, transactionId });
  },

  bulkAdjustStocks: (items: Array<{ barcode: string; amount: number }>) => {
    const transactionId = (window.crypto && window.crypto.randomUUID) 
      ? window.crypto.randomUUID() 
      : Math.random().toString(36).substring(2) + Date.now().toString(36);
    
    console.log(`[API] Bulk adjust requested: count=${items.length}, transactionId=${transactionId}`);
    return gatewayClient.post(`/api/stocks/bulk-increase`, { items, transactionId });
  },
};

console.log('[API] stockApi module loaded. Keys:', Object.keys(stockApi));
