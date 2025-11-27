import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { operatorApi } from '../services/api';
import type { Operation, SetupSheet, ToolAssemblyAvailability } from '../types';

export default function OperationPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [operation, setOperation] = useState<Operation | null>(null);
  const [setupSheet, setSetupSheet] = useState<SetupSheet | null>(null);
  const [tooling, setTooling] = useState<ToolAssemblyAvailability[]>([]);
  const [showLocations, setShowLocations] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadOperationData();
  }, [id, showLocations]);

  const loadOperationData = async () => {
    if (!id) return;

    try {
      setLoading(true);
      setError(null);

      const operationId = parseInt(id);
      const [opData, sheetData, toolingData] = await Promise.all([
        operatorApi.getOperation(operationId),
        operatorApi.getSetupSheet(operationId).catch(() => null),
        operatorApi.getOperationTooling(operationId, showLocations)
      ]);

      setOperation(opData);
      setSetupSheet(sheetData);
      setTooling(toolingData);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load operation data');
    } finally {
      setLoading(false);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  const getAvailabilityColor = (status: string) => {
    switch (status) {
      case 'FullyAvailable':
        return 'bg-green-900 border-green-700 text-green-200';
      case 'PartiallyAvailable':
        return 'bg-yellow-900 border-yellow-700 text-yellow-200';
      case 'NotAvailable':
        return 'bg-red-900 border-red-700 text-red-200';
      default:
        return 'bg-gray-800 border-gray-700 text-gray-300';
    }
  };

  const getAvailabilityLabel = (status: string) => {
    switch (status) {
      case 'FullyAvailable':
        return '✓ Fully Available';
      case 'PartiallyAvailable':
        return '⚠ Partially Available';
      case 'NotAvailable':
        return '✗ Not Available';
      default:
        return 'Unknown';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-16 w-16 border-b-4 border-blue-500"></div>
          <p className="text-2xl text-gray-300 mt-4">Loading...</p>
        </div>
      </div>
    );
  }

  if (error || !operation) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center p-8">
        <div className="bg-red-900 border border-red-700 rounded-lg p-8 max-w-2xl">
          <p className="text-2xl text-red-200">{error || 'Operation not found'}</p>
          <button
            onClick={() => navigate('/')}
            className="mt-6 bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg text-xl"
          >
            Back to Scan
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900">
      {/* Header */}
      <div className="bg-gray-800 border-b border-gray-700 p-6 print:hidden">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold text-white">{operation.operationName}</h1>
            {operation.espritProgramName && (
              <p className="text-gray-400 mt-1">Program: {operation.espritProgramName}</p>
            )}
          </div>
          <button
            onClick={() => navigate('/')}
            className="bg-gray-700 hover:bg-gray-600 text-white px-6 py-3 rounded-lg text-lg"
          >
            ← Back to Scan
          </button>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-6 grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Left Panel - Setup Sheet */}
        <div className="bg-gray-800 rounded-lg shadow-xl p-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-2xl font-bold text-white">Setup Sheet</h2>
            {setupSheet && (
              <button
                onClick={handlePrint}
                className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                </svg>
                Print
              </button>
            )}
          </div>

          {setupSheet ? (
            <div className="bg-gray-700 rounded-lg p-4 min-h-96">
              {setupSheet.format === 'PDF' && (
                <iframe
                  src={setupSheet.url || setupSheet.filePath}
                  className="w-full h-96 border-0 rounded"
                  title="Setup Sheet"
                />
              )}
              {setupSheet.format === 'HTML' && setupSheet.url && (
                <iframe
                  src={setupSheet.url}
                  className="w-full h-96 border-0 rounded"
                  title="Setup Sheet"
                />
              )}
              {setupSheet.description && (
                <p className="text-gray-300 mt-4">{setupSheet.description}</p>
              )}
            </div>
          ) : (
            <div className="bg-gray-700 rounded-lg p-8 text-center text-gray-400">
              <p className="text-xl">No setup sheet available</p>
            </div>
          )}
        </div>

        {/* Right Panel - Tooling */}
        <div className="bg-gray-800 rounded-lg shadow-xl p-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-2xl font-bold text-white">Required Tooling</h2>
            <button
              onClick={() => setShowLocations(!showLocations)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                showLocations
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
              }`}
            >
              {showLocations ? 'Hide' : 'Show'} Locations
            </button>
          </div>

          <div className="space-y-4">
            {tooling.map((item) => (
              <div
                key={item.assembly.toolAssemblyId}
                className={`border-2 rounded-lg p-4 ${getAvailabilityColor(item.availabilityStatus)}`}
              >
                <div className="flex justify-between items-start mb-2">
                  <div>
                    <h3 className="text-xl font-semibold">
                      {item.assembly.toolNumber ? `${item.assembly.toolNumber} - ` : ''}
                      {item.assembly.assemblyName}
                    </h3>
                    <p className="text-sm opacity-90">{item.assembly.description}</p>
                  </div>
                  <span className="text-lg font-bold whitespace-nowrap ml-4">
                    {getAvailabilityLabel(item.availabilityStatus)}
                  </span>
                </div>

                {showLocations && item.componentAvailability.length > 0 && (
                  <div className="mt-4 space-y-2 border-t border-current opacity-75 pt-3">
                    {item.componentAvailability.map((comp, idx) => (
                      <div key={idx} className="text-sm">
                        <div className="font-medium">
                          {comp.component.componentType}: {comp.component.componentCode}
                          <span className="ml-2">
                            ({comp.quantityAvailable} / {comp.quantityRequired})
                          </span>
                        </div>
                        {comp.locations.map((loc, locIdx) => (
                          <div key={locIdx} className="ml-4 opacity-75">
                            • {loc.locationDescription} ({loc.status}) - Qty: {loc.quantity}
                          </div>
                        ))}
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}

            {tooling.length === 0 && (
              <div className="text-center py-8 text-gray-400">
                <p className="text-xl">No tooling requirements defined</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
