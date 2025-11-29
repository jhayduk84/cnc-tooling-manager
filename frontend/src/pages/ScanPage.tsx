import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { operatorApi } from '../services/api';
import type { Operation } from '../types';

export default function ScanPage() {
  const [barcode, setBarcode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [operations, setOperations] = useState<Operation[]>([]);
  const inputRef = useRef<HTMLInputElement>(null);
  const navigate = useNavigate();

  useEffect(() => {
    // Auto-focus input on mount
    inputRef.current?.focus();
  }, []);

  const handleScan = async (scannedValue: string) => {
    setLoading(true);
    setError(null);
    setOperations([]);

    try {
      // Parse barcode - format could be "PARTNUM" or "PARTNUM|REV"
      const parts = scannedValue.split('|');
      const partNumber = parts[0];
      const revision = parts.length > 1 ? parts[1] : undefined;

      // Look up operations
      const ops = await operatorApi.getPartOperations(partNumber, revision);
      
      if (ops.length === 0) {
        setError(`No operations found for part ${partNumber}`);
      } else if (ops.length === 1) {
        // Auto-navigate if only one operation
        navigate(`/operation/${ops[0].operationId}`);
      } else {
        // Show operation selection
        setOperations(ops);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to look up part. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (barcode.trim()) {
      handleScan(barcode.trim());
    }
  };

  const handleOperationSelect = (operationId: number) => {
    navigate(`/operation/${operationId}`);
  };

  return (
    <div className="min-h-screen bg-black flex flex-col items-center justify-center p-8">
      {/* Admin Link */}
      <div className="absolute top-4 right-4">
        <button
          onClick={() => navigate('/admin')}
          className="px-4 py-2 bg-gray-800 text-white rounded hover:bg-gray-700 flex items-center gap-2"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          Admin
        </button>
      </div>
      <div className="w-full max-w-4xl">
        {/* Company Logo */}
        <div className="flex justify-center mb-2">
          <img 
            src="/logo.png" 
            alt="Company Logo" 
            className="h-256 w-auto"
          />
        </div>
        
        <h1 className="text-6xl font-bold text-white text-center mb-12">
          CNC Tooling Manager
        </h1>

        <div className="bg-gray-800 rounded-lg shadow-xl p-8">
          <form onSubmit={handleSubmit} className="mb-8">
            <label htmlFor="barcode" className="block text-3xl font-medium text-gray-300 mb-4">
              Scan Part Number
            </label>
            <input
              ref={inputRef}
              id="barcode"
              type="text"
              value={barcode}
              onChange={(e) => setBarcode(e.target.value)}
              className="w-full px-6 py-6 text-4xl bg-gray-700 border border-gray-600 rounded-lg text-white focus:ring-4 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Scan barcode or enter part number..."
              autoComplete="off"
              disabled={loading}
            />
          </form>

          {loading && (
            <div className="text-center py-12">
              <div className="inline-block animate-spin rounded-full h-16 w-16 border-b-4 border-blue-500"></div>
              <p className="text-2xl text-gray-300 mt-4">Looking up part...</p>
            </div>
          )}

          {error && (
            <div className="bg-red-900 border border-red-700 rounded-lg p-6 mb-6">
              <p className="text-2xl text-red-200">{error}</p>
            </div>
          )}

          {operations.length > 0 && (
            <div>
              <h2 className="text-3xl font-semibold text-white mb-6">
                Select Operation
              </h2>
              <div className="grid gap-4">
                {operations.map((op) => (
                  <button
                    key={op.operationId}
                    onClick={() => handleOperationSelect(op.operationId)}
                    className="bg-blue-600 hover:bg-blue-700 text-white rounded-lg p-6 text-left transition-colors"
                  >
                    <div className="text-3xl font-bold mb-2">{op.operationName}</div>
                    {op.espritProgramName && (
                      <div className="text-xl text-blue-200">Program: {op.espritProgramName}</div>
                    )}
                    {op.notes && (
                      <div className="text-lg text-blue-200 mt-2">{op.notes}</div>
                    )}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className="text-center mt-8">
          <button
            onClick={() => navigate('/admin')}
            className="text-gray-400 hover:text-gray-200 text-lg underline"
          >
            Admin Panel
          </button>
        </div>
      </div>
    </div>
  );
}
