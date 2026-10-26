import { Routes, Route, Link } from 'react-router-dom';
import Dashboard from './pages/Dashboard';
import Products from './pages/Products';
import Orders from './pages/Orders';
import ErpIntegration from './pages/ErpIntegration';

function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-blue-600 text-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center">
              <span className="text-xl font-bold">FastenerFlow</span>
            </div>
            <div className="flex space-x-4">
              <Link to="/" className="px-3 py-2 rounded-md hover:bg-blue-700 transition">
                Dashboard
              </Link>
              <Link to="/products" className="px-3 py-2 rounded-md hover:bg-blue-700 transition">
                Products
              </Link>
              <Link to="/orders" className="px-3 py-2 rounded-md hover:bg-blue-700 transition">
                Orders
              </Link>
              <Link to="/erp" className="px-3 py-2 rounded-md hover:bg-blue-700 transition">
                ERP Integration
              </Link>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 py-8">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/products" element={<Products />} />
          <Route path="/orders" element={<Orders />} />
          <Route path="/erp" element={<ErpIntegration />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
