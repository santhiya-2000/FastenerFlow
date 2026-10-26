import api from './axios';

export interface Product {
  id: number;
  sku: string;
  name: string;
  category: string;
  unitPrice: number;
  quantity: number;
  reorderLevel: number;
}

export interface CreateProduct {
  sku: string;
  name: string;
  category: string;
  unitPrice: number;
  quantity: number;
  reorderLevel: number;
}

export interface UpdateProduct {
  sku: string;
  name: string;
  category: string;
  unitPrice: number;
  quantity: number;
  reorderLevel: number;
}

export const productsApi = {
  getAll: async (): Promise<Product[]> => {
    const response = await api.get<Product[]>('/products');
    return response.data;
  },

  getById: async (id: number): Promise<Product> => {
    const response = await api.get<Product>(`/products/${id}`);
    return response.data;
  },

  getLowStock: async (): Promise<Product[]> => {
    const response = await api.get<Product[]>('/products/low-stock');
    return response.data;
  },

  create: async (product: CreateProduct): Promise<Product> => {
    const response = await api.post<Product>('/products', product);
    return response.data;
  },

  update: async (id: number, product: UpdateProduct): Promise<Product> => {
    const response = await api.put<Product>(`/products/${id}`, product);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/products/${id}`);
  },
};
