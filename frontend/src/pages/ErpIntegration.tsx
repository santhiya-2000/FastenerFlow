import { useState, useEffect } from 'react';
import { erpApi } from '../api/erp';
import type { ErpSyncResult, ErpSyncLog } from '../api/erp';

export default function ErpIntegration() {
  const [syncHistory, setSyncHistory] = useState<ErpSyncLog[]>([]);
  const [isSyncing, setIsSyncing] = useState(false);
  const [syncResult, setSyncResult] = useState<ErpSyncResult | null>(null);

  useEffect(() => {
    loadSyncHistory();
  }, []);

  const loadSyncHistory = async () => {
    try {
      const data = await erpApi.getSyncHistory();
      setSyncHistory(data);
    } catch (error) {
      console.error('Error loading sync history:', error);
    }
  };

  const handleSync = async () => {
    setIsSyncing(true);
    setSyncResult(null);
    try {
      const result = await erpApi.sync();
      setSyncResult(result);
      loadSyncHistory();
    } catch (error: any) {
      setSyncResult({
        success: false,
        productsUpdated: 0,
        productsFailed: 0,
        discrepancies: [],
        message: error.response?.data?.message || 'Sync failed. Please try again.',
      });
    } finally {
      setIsSyncing(false);
    }
  };

  return (
    <div>
      <div className="mb-8 bg-yellow-50 border-l-4 border-yellow-400 p-4">
        <div className="flex">
          <div className="flex-shrink-0">
            <span className="text-yellow-400">⚠️</span>
          </div>
          <div className="ml-3">
            <p className="text-sm text-yellow-700">
              <strong>Mock ERP Integration:</strong> This is a simulated ERP integration for demonstration purposes.
              The mock ERP API simulates Microsoft Dynamics 365 Business Central integration patterns.
            </p>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">ERP Synchronization</h2>
        <p className="text-gray-600 mb-4">
          Synchronize inventory data with the mock ERP system. This simulates integration with Microsoft Dynamics 365 Business Central.
        </p>
        <button
          onClick={handleSync}
          disabled={isSyncing}
          className="bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed"
        >
          {isSyncing ? 'Syncing...' : 'Sync with ERP (Demo)'}
        </button>

        {syncResult && (
          <div
            className={`mt-4 p-4 rounded-md ${
              syncResult.success ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
            }`}
          >
            <h3 className={`font-semibold ${syncResult.success ? 'text-green-800' : 'text-red-800'}`}>
              {syncResult.success ? 'Sync Successful' : 'Sync Failed'}
            </h3>
            <p className={`mt-2 ${syncResult.success ? 'text-green-700' : 'text-red-700'}`}>
              {syncResult.message}
            </p>
            {syncResult.productsUpdated > 0 && (
              <p className="mt-2 text-green-700">
                Products Updated: {syncResult.productsUpdated}
              </p>
            )}
            {syncResult.productsFailed > 0 && (
              <p className="mt-2 text-red-700">
                Products Failed: {syncResult.productsFailed}
              </p>
            )}
            {syncResult.discrepancies.length > 0 && (
              <div className="mt-4">
                <h4 className="font-semibold text-gray-900 mb-2">Discrepancies:</h4>
                <ul className="list-disc list-inside text-sm text-gray-700 space-y-1">
                  {syncResult.discrepancies.map((discrepancy, index) => (
                    <li key={index}>{discrepancy}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>

      <div className="bg-white rounded-lg shadow-lg p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Synchronization History</h2>
        {syncHistory.length === 0 ? (
          <p className="text-gray-600">No synchronization history available.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Updated
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Failed
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Details
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {syncHistory.map((log) => (
                  <tr key={log.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {new Date(log.syncDate).toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          log.status === 'Success'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {log.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {log.productsUpdated}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {log.productsFailed}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900 max-w-xs truncate">
                      {log.details || '-'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
