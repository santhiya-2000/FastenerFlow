import api from './axios';

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Order {
  id: number;
  orderDate: string;
  status: string;
  orderItems: OrderItem[];
}

export interface CreateOrderItem {
  productId: number;
  quantity: number;
}

export interface CreateOrder {
  orderItems: CreateOrderItem[];
}

export const ordersApi = {
  getAll: async (): Promise<Order[]> => {
    const response = await api.get<Order[]>('/orders');
    return response.data;
  },

  getById: async (id: number): Promise<Order> => {
    const response = await api.get<Order>(`/orders/${id}`);
    return response.data;
  },

  create: async (order: CreateOrder): Promise<Order> => {
    const response = await api.post<Order>('/orders', order);
    return response.data;
  },
};
