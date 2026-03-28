export interface Product {
  barcode: string;
  name: string;
  class: string;
  color: string;
  size: string;
  price: number;
  id?: string;
}

export interface Stock {
  barcode: string;
  warehouseQuantity: number;
  availableQuantity?: number;
  source: string;
  lastUpdatedAt: string;
}

export interface ProductWithStock extends Product {
  stock?: Stock;
}
