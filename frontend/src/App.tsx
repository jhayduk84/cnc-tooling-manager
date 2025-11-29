import { BrowserRouter, Routes, Route } from 'react-router-dom';
import ScanPage from './pages/ScanPage';
import OperationPage from './pages/OperationPage';
import { AdminPage } from './pages/AdminPage';
import AdminDashboard from './pages/AdminDashboard';
import ComponentsAdmin from './pages/ComponentsAdmin';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<ScanPage />} />
        <Route path="/operation/:id" element={<OperationPage />} />
        <Route path="/admin" element={<AdminPage />} />
        <Route path="/admin/dashboard" element={<AdminDashboard />} />
        <Route path="/admin/components" element={<ComponentsAdmin />} />
        {/* Placeholder routes for other admin pages */}
        <Route path="/admin/assemblies" element={<div className="min-h-screen bg-gray-900 text-white p-8">Tool Assemblies Admin - Coming Soon</div>} />
        <Route path="/admin/machines" element={<div className="min-h-screen bg-gray-900 text-white p-8">Machines Admin - Coming Soon</div>} />
        <Route path="/admin/locations" element={<div className="min-h-screen bg-gray-900 text-white p-8">Inventory Locations Admin - Coming Soon</div>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
