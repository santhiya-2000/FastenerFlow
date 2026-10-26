import api from './axios';

export interface ErpSyncResult {
  success: boolean;
  productsUpdated: number;
  productsFailed: number;
  discrepancies: string[];
  message: string;
}

export interface ErpSyncLog {
  id: number;
  syncDate: string;
  status: string;
  productsUpdated: number;
  productsFailed: number;
  details: string | null;
}

export const erpApi = {
  sync: async (): Promise<ErpSyncResult> => {
    const response = await api.post<ErpSyncResult>('/erp/sync');
    return response.data;
  },

  getSyncHistory: async (): Promise<ErpSyncLog[]> => {
    const response = await api.get<ErpSyncLog[]>('/erp/sync-history');
    return response.data;
  },
};
