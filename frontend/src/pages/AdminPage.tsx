import React, { useState } from 'react';
import { PartsAdmin } from './PartsAdmin';
import { MachinesAdmin } from './MachinesAdmin';
import { ToolsAdmin } from './ToolsAdmin';

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
          {activeTab === 'machines' && (
            <div className="space-y-4">
              <div className="bg-yellow-900 bg-opacity-30 border border-yellow-700 rounded-lg p-4">
                <div className="flex items-center gap-2">
                  <svg className="w-5 h-5 text-yellow-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                  <span className="text-yellow-200 font-semibold">Under Development</span>
                </div>
                <p className="text-yellow-100 text-sm mt-2">This feature is currently being developed and will be available soon.</p>
              </div>
              <MachinesAdmin />
            </div>
          )}
          {activeTab === 'tools' && <ToolsAdmin />}
        </div>
      </div>
    </div>
  );
};
