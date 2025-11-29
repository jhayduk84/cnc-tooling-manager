import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { ExcelViewer } from '../components/ExcelViewer';

interface SetupSheet {
  id: number;
  setupSheetId: number;
  operationId: number;
  instructions: string;
  filePath: string;
  format: string;
  description: string;
  isActive: boolean;
  isArchived: boolean;
  archiveReason?: string;
  version: number;
  createdAt: string;
  archivedAt?: string;
  operation?: {
    operationId: number;
    operationName: string;
    partRevision?: {
      part?: {
        partNumber: string;
        description: string;
      };
      revisionNumber: string;
    };
  };
}

export const SetupSheetsAdmin: React.FC = () => {
  const [setupSheets, setSetupSheets] = useState<SetupSheet[]>([]);
  const [loading, setLoading] = useState(true);
  const [showUploadForm, setShowUploadForm] = useState(false);
  const [viewingSheet, setViewingSheet] = useState<SetupSheet | null>(null);
  const [showArchived, setShowArchived] = useState(false);
  const [uploadData, setUploadData] = useState({
    partNumber: '',
    partDescription: '',
    operationName: '',
    instructions: '',
    createNewOperation: false,
    archiveReason: '',
    file: null as File | null,
  });

  useEffect(() => {
    loadData();
  }, [showArchived]);

  const loadData = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/setupsheets?includeArchived=${showArchived}`);
      setSetupSheets(response.data);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setUploadData({ ...uploadData, file: e.target.files[0] });
    }
  };

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!uploadData.file || !uploadData.partNumber || !uploadData.operationName) {
      alert('Please fill in all required fields');
      return;
    }

    try {
      const formData = new FormData();
      formData.append('file', uploadData.file);
      formData.append('partNumber', uploadData.partNumber);
      formData.append('partDescription', uploadData.partDescription);
      formData.append('operationName', uploadData.operationName);
      formData.append('instructions', uploadData.instructions);
      formData.append('createNewOperation', uploadData.createNewOperation.toString());
      if (uploadData.archiveReason) {
        formData.append('archiveReason', uploadData.archiveReason);
      }

      await api.post('/setupsheets/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      setShowUploadForm(false);
      setUploadData({
        partNumber: '',
        partDescription: '',
        operationName: '',
        instructions: '',
        createNewOperation: false,
        archiveReason: '',
        file: null,
      });
      loadData();
    } catch (error) {
      console.error('Failed to upload setup sheet:', error);
      alert('Failed to upload setup sheet');
    }
  };

  const handleArchive = async (id: number) => {
    const reason = prompt('Enter archive reason:');
    if (!reason) return;

    try {
      await api.post(`/setupsheets/${id}/archive`, JSON.stringify(reason), {
        headers: { 'Content-Type': 'application/json' },
      });
      loadData();
    } catch (error) {
      console.error('Failed to archive setup sheet:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to permanently delete this setup sheet?')) return;

    try {
      await api.delete(`/setupsheets/${id}`);
      loadData();
    } catch (error) {
      console.error('Failed to delete setup sheet:', error);
    }
  };

  const handleDownload = async (sheet: SetupSheet) => {
    try {
      const response = await api.get(`/setupsheets/download/${sheet.id}`, {
        responseType: 'blob',
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', sheet.description);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      console.error('Failed to download setup sheet:', error);
    }
  };

  if (loading) {
    return <div className="text-center py-8 text-gray-400">Loading...</div>;
  }

  return (
    <div className="bg-gray-800 rounded-lg shadow">
      <div className="px-6 py-4 border-b border-gray-700 flex justify-between items-center">
        <div className="flex items-center gap-4">
          <h2 className="text-xl font-semibold text-white">Setup Sheets by Part</h2>
          <label className="flex items-center gap-2 text-sm text-gray-300">
            <input
              type="checkbox"
              checked={showArchived}
              onChange={(e) => setShowArchived(e.target.checked)}
              className="rounded"
            />
            Show Archived
          </label>
        </div>
        <button
          onClick={() => setShowUploadForm(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-2"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
          Upload Setup Sheet
        </button>
      </div>

      {/* Upload Form Modal */}
      {showUploadForm && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 overflow-y-auto">
          <div className="bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl mx-4 my-8">
            <div className="px-6 py-4 border-b border-gray-700">
              <h3 className="text-lg font-semibold text-white">Upload Setup Sheet</h3>
              <p className="text-sm text-gray-400 mt-1">
                Part and operation will be created if they don't exist. Existing setup sheets will be archived.
              </p>
            </div>
            <form onSubmit={handleUpload} className="px-6 py-4 max-h-[70vh] overflow-y-auto">
              <div className="grid grid-cols-2 gap-4">
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Part Number *
                  </label>
                  <input
                    type="text"
                    required
                    value={uploadData.partNumber}
                    onChange={(e) => setUploadData({ ...uploadData, partNumber: e.target.value })}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., 2220-0137"
                  />
                </div>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Part Description
                  </label>
                  <input
                    type="text"
                    value={uploadData.partDescription}
                    onChange={(e) => setUploadData({ ...uploadData, partDescription: e.target.value })}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Optional"
                  />
                </div>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Operation Name *
                </label>
                <input
                  type="text"
                  required
                  value={uploadData.operationName}
                  onChange={(e) => setUploadData({ ...uploadData, operationName: e.target.value })}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., Mill Operation A"
                />
              </div>

              <div className="mb-4">
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={uploadData.createNewOperation}
                    onChange={(e) => setUploadData({ ...uploadData, createNewOperation: e.target.checked })}
                    className="rounded"
                  />
                  <span className="text-sm text-gray-300">Create as new operation (don't update existing)</span>
                </label>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Excel File *
                </label>
                <input
                  type="file"
                  accept=".xlsx,.xls"
                  required
                  onChange={handleFileChange}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="mt-1 text-sm text-gray-400">Only .xlsx and .xls files are allowed</p>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Setup Instructions
                </label>
                <textarea
                  rows={3}
                  value={uploadData.instructions}
                  onChange={(e) => setUploadData({ ...uploadData, instructions: e.target.value })}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Optional setup instructions..."
                />
              </div>

              {!uploadData.createNewOperation && (
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Archive Reason (if updating existing operation)
                  </label>
                  <input
                    type="text"
                    value={uploadData.archiveReason}
                    onChange={(e) => setUploadData({ ...uploadData, archiveReason: e.target.value })}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., Updated tooling, Revised process"
                  />
                </div>
              )}

              <div className="flex gap-2 justify-end mt-6">
                <button
                  type="button"
                  onClick={() => {
                    setShowUploadForm(false);
                    setUploadData({
                      partNumber: '',
                      partDescription: '',
                      operationName: '',
                      instructions: '',
                      createNewOperation: false,
                      archiveReason: '',
                      file: null,
                    });
                  }}
                  className="px-4 py-2 border border-gray-600 text-gray-300 rounded hover:bg-gray-700"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Upload
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Viewer Modal */}
      {viewingSheet && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
          <div className="bg-gray-800 rounded-lg shadow-xl w-full max-w-6xl max-h-[90vh] overflow-hidden flex flex-col">
            <div className="px-6 py-4 border-b border-gray-700 flex justify-between items-center">
              <div>
                <h3 className="text-lg font-semibold text-white">{viewingSheet.description}</h3>
                <p className="text-sm text-gray-400">
                  Part: {viewingSheet.operation?.partRevision?.part?.partNumber || 'N/A'} | 
                  Operation: {viewingSheet.operation?.operationName || 'N/A'} |
                  Version: {viewingSheet.version}
                  {viewingSheet.isArchived && (
                    <span className="ml-2 px-2 py-1 text-xs bg-yellow-900 text-yellow-200 rounded">ARCHIVED</span>
                  )}
                </p>
              </div>
              <button
                onClick={() => setViewingSheet(null)}
                className="text-gray-400 hover:text-gray-200"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div className="flex-1 overflow-auto p-6">
              {viewingSheet.instructions && (
                <div className="mb-4 p-4 bg-yellow-900 border border-yellow-700 rounded">
                  <p className="text-sm font-medium text-yellow-200">Instructions:</p>
                  <p className="text-sm text-yellow-100">{viewingSheet.instructions}</p>
                </div>
              )}
              {viewingSheet.isArchived && viewingSheet.archiveReason && (
                <div className="mb-4 p-4 bg-red-900 border border-red-700 rounded">
                  <p className="text-sm font-medium text-red-200">Archive Reason:</p>
                  <p className="text-sm text-red-100">{viewingSheet.archiveReason}</p>
                  <p className="text-xs text-red-300 mt-1">
                    Archived: {new Date(viewingSheet.archivedAt!).toLocaleString()}
                  </p>
                </div>
              )}
              <ExcelViewer fileUrl={`http://localhost:5000/api/setupsheets/view/${viewingSheet.id}`} />
            </div>
          </div>
        </div>
      )}

      {/* Setup Sheets Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-700">
          <thead className="bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Part Number
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Operation
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                File Name
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Version
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Date
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-gray-800 divide-y divide-gray-700">
            {setupSheets.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-4 text-center text-gray-400">
                  No setup sheets found. Click "Upload Setup Sheet" to add one.
                </td>
              </tr>
            ) : (
              setupSheets.map((sheet) => (
                <tr key={sheet.id} className={`hover:bg-gray-700 ${sheet.isArchived ? 'opacity-60' : ''}`}>
                  <td className="px-6 py-4 whitespace-nowrap font-medium text-white">
                    {sheet.operation?.partRevision?.part?.partNumber || 'N/A'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-gray-300">
                    {sheet.operation?.operationName || 'N/A'}
                  </td>
                  <td className="px-6 py-4 text-gray-300">{sheet.description}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                    v{sheet.version}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {sheet.isArchived ? (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-yellow-900 text-yellow-200">
                        Archived
                      </span>
                    ) : (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-900 text-green-200">
                        Active
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                    {new Date(sheet.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => setViewingSheet(sheet)}
                      className="text-blue-400 hover:text-blue-300 mr-3"
                    >
                      View
                    </button>
                    <button
                      onClick={() => handleDownload(sheet)}
                      className="text-green-400 hover:text-green-300 mr-3"
                    >
                      Download
                    </button>
                    {!sheet.isArchived && (
                      <button
                        onClick={() => handleArchive(sheet.id)}
                        className="text-yellow-400 hover:text-yellow-300 mr-3"
                      >
                        Archive
                      </button>
                    )}
                    <button
                      onClick={() => handleDelete(sheet.id)}
                      className="text-red-400 hover:text-red-300"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
