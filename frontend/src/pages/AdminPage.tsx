import React, { useState } from 'react';
import { PartsAdmin } from './PartsAdmin';
import { MachinesAdmin } from './MachinesAdmin';

export const AdminPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'parts' | 'machines' | 'tools'>('parts');

  const tabs = [
    { id: 'parts' as const, label: 'Parts', icon: '🔧' },
    { id: 'machines' as const, label: 'Machines', icon: '⚙️' },
    { id: 'tools' as const, label: 'Tools', icon: '🔨' },
  ];

  return (
    <div className="min-h-screen bg-gray-900">
      {/* Header */}
      <header className="bg-gray-800 shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <h1 className="text-3xl font-bold text-white">Admin Dashboard</h1>
        </div>
      </header>

      {/* Navigation Tabs */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-6">
        <div className="border-b border-gray-700">
          <nav className="-mb-px flex space-x-8">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`
                  whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                  ${activeTab === tab.id
                    ? 'border-blue-500 text-blue-400'
                    : 'border-transparent text-gray-400 hover:text-gray-200 hover:border-gray-500'
                  }
                `}
              >
                <span className="text-xl">{tab.icon}</span>
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Tab Content */}
        <div className="mt-6 pb-12">
          {activeTab === 'parts' && <PartsAdmin />}
          {activeTab === 'machines' && <MachinesAdmin />}
          {activeTab === 'tools' && (
            <div className="bg-gray-800 rounded-lg shadow p-6">
              <h2 className="text-xl font-semibold mb-4 text-white">Tools Management</h2>
              <p className="text-gray-400">Coming soon...</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
