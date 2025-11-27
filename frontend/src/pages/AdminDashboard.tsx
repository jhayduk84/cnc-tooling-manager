import { Link } from 'react-router-dom';

export default function AdminDashboard() {
  const adminSections = [
    {
      title: 'Tool Components',
      description: 'Manage cutters, holders, collets, extensions, inserts',
      path: '/admin/components',
      icon: '🔧',
    },
    {
      title: 'Tool Assemblies',
      description: 'Manage complete tool assemblies',
      path: '/admin/assemblies',
      icon: '🛠️',
    },
    {
      title: 'Machines',
      description: 'Manage CNC machines and tool locations',
      path: '/admin/machines',
      icon: '⚙️',
    },
    {
      title: 'Inventory Locations',
      description: 'Manage crib locations and storage areas',
      path: '/admin/locations',
      icon: '📦',
    },
    {
      title: 'Parts & Operations',
      description: 'Manage parts, revisions, and operations (Coming Soon)',
      path: '#',
      icon: '📋',
      disabled: true,
    },
    {
      title: 'Setup Kits',
      description: 'Manage pre-staged tooling kits (Coming Soon)',
      path: '#',
      icon: '📦',
      disabled: true,
    },
  ];

  return (
    <div className="min-h-screen bg-gray-900">
      <div className="bg-gray-800 border-b border-gray-700 p-6">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <h1 className="text-3xl font-bold text-white">Admin Dashboard</h1>
          <Link
            to="/"
            className="bg-gray-700 hover:bg-gray-600 text-white px-6 py-3 rounded-lg"
          >
            ← Back to Operator View
          </Link>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {adminSections.map((section) => (
            section.disabled ? (
              <div
                key={section.title}
                className="bg-gray-800 border border-gray-700 rounded-lg p-6 opacity-50 cursor-not-allowed"
              >
                <div className="text-4xl mb-4">{section.icon}</div>
                <h2 className="text-2xl font-bold text-white mb-2">{section.title}</h2>
                <p className="text-gray-400">{section.description}</p>
              </div>
            ) : (
              <Link
                key={section.title}
                to={section.path}
                className="bg-gray-800 border border-gray-700 hover:border-blue-500 rounded-lg p-6 transition-colors group"
              >
                <div className="text-4xl mb-4">{section.icon}</div>
                <h2 className="text-2xl font-bold text-white mb-2 group-hover:text-blue-400">
                  {section.title}
                </h2>
                <p className="text-gray-400">{section.description}</p>
              </Link>
            )
          ))}
        </div>
      </div>
    </div>
  );
}
