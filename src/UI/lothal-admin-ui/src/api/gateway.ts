import axios from 'axios';
import type { Product, Stock } from '../types/product';

const gatewayClient = axios.create({
  baseURL: 'http://localhost:5024', // API Gateway URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export const productApi = {
  getAll: (from = 0, size = 100) => 
    gatewayClient.get<Product[]>(`/api/gateway/products?from=${from}&size=${size}`),
  
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
  
  release: (barcode: string, quantity: number) => 
    gatewayClient.post(`/api/stocks/${barcode}/release`, { quantity }),
};
