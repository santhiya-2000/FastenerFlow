import { useState, useEffect } from 'react';
import { productsApi } from '../api/products';
import { ordersApi } from '../api/orders';
import { erpApi } from '../api/erp';
import { Link } from 'react-router-dom';

export default function Dashboard() {
  const [stats, setStats] = useState({
    totalProducts: 0,
    totalInventory: 0,
    lowStockCount: 0,
    totalOrders: 0,
    lastSyncStatus: 'Never synced',
  });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [products, orders, syncHistory] = await Promise.all([
        productsApi.getAll(),
        ordersApi.getAll(),
        erpApi.getSyncHistory(),
      ]);

      const lowStock = await productsApi.getLowStock();

      setStats({
        totalProducts: products.length,
        totalInventory: products.reduce((sum, p) => sum + p.quantity, 0),
        lowStockCount: lowStock.length,
        totalOrders: orders.length,
        lastSyncStatus: syncHistory.length > 0
          ? `${syncHistory[0].status} (${new Date(syncHistory[0].syncDate).toLocaleString()})`
          : 'Never synced',
      });
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    }
  };

  const statCards = [
    {
      title: 'Total Products',
      value: stats.totalProducts,
      color: 'bg-blue-500',
      link: '/products',
    },
    {
      title: 'Total Inventory Units',
      value: stats.totalInventory,
      color: 'bg-green-500',
      link: '/products',
    },
    {
      title: 'Low Stock Products',
      value: stats.lowStockCount,
      color: 'bg-yellow-500',
      link: '/products',
    },
    {
      title: 'Total Orders',
      value: stats.totalOrders,
      color: 'bg-purple-500',
      link: '/orders',
    },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Dashboard</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {statCards.map((card) => (
          <Link key={card.title} to={card.link}>
            <div className={`${card.color} rounded-lg shadow-lg p-6 text-white hover:opacity-90 transition`}>
              <h3 className="text-lg font-semibold mb-2">{card.title}</h3>
              <p className="text-4xl font-bold">{card.value}</p>
            </div>
          </Link>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow-lg p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">ERP Sync Status</h2>
        <p className="text-gray-600">{stats.lastSyncStatus}</p>
        <Link
          to="/erp"
          className="mt-4 inline-block bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition"
        >
          Go to ERP Integration
        </Link>
      </div>
    </div>
  );
}
