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
  };
}

interface Operation {
  operationId: number;
  operationName: string;
  setupSheets?: SetupSheet[];
}

interface PartRevision {
  partRevisionId: number;
  revisionNumber: string;
  operations?: Operation[];
}

interface Part {
  partId: number;
  partNumber: string;
  description: string;
  customer?: string;
  createdAt: string;
  isActive: boolean;
  revisions?: PartRevision[];
}

export const PartsAdmin: React.FC = () => {
  const [parts, setParts] = useState<Part[]>([]);
  const [filteredParts, setFilteredParts] = useState<Part[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [showUploadForm, setShowUploadForm] = useState(false);
  const [editingPart, setEditingPart] = useState<Part | null>(null);
  const [expandedPart, setExpandedPart] = useState<number | null>(null);
  const [viewingSheet, setViewingSheet] = useState<SetupSheet | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [customerFilter, setCustomerFilter] = useState('');
  const [showArchived, setShowArchived] = useState(false);
  const [uploadingPartId, setUploadingPartId] = useState<number | null>(null);
  
  const [formData, setFormData] = useState({
    partNumber: '',
    description: '',
    customer: '',
    isActive: true,
  });

  const [uploadData, setUploadData] = useState({
    operationName: '',
    instructions: '',
    createNewOperation: false,
    archiveReason: '',
    file: null as File | null,
    filePath: '',
    useLinkMode: false,
  });

  useEffect(() => {
    loadParts();
  }, []);

  useEffect(() => {
    filterParts();
  }, [parts, searchTerm, customerFilter]);

  const loadParts = async () => {
    try {
      setLoading(true);
      const response = await api.get('/parts');
      setParts(response.data);
    } catch (error) {
      console.error('Failed to load parts:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadPartDetails = async (partId: number) => {
    try {
      const response = await api.get(`/setupsheets/part/${partId}`);
      const updatedParts = parts.map(p => 
        p.partId === partId ? { ...p, revisions: response.data.revisions } : p
      );
      setParts(updatedParts);
    } catch (error) {
      console.error('Failed to load part details:', error);
    }
  };

  const filterParts = () => {
    let filtered = [...parts];
    
    if (searchTerm) {
      filtered = filtered.filter(p => 
        p.partNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.description.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
    
    if (customerFilter) {
      filtered = filtered.filter(p => 
        p.customer?.toLowerCase().includes(customerFilter.toLowerCase())
      );
    }
    
    setFilteredParts(filtered);
  };

  const getUniqueCustomers = () => {
    const customers = parts
      .map(p => p.customer)
      .filter((c): c is string => !!c)
      .filter((v, i, a) => a.indexOf(v) === i)
      .sort();
    return customers;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingPart) {
        await api.put(`/parts/${editingPart.partId}`, {
          ...editingPart,
          ...formData,
        });
      } else {
        await api.post('/parts', formData);
      }
      setShowForm(false);
      setEditingPart(null);
      setFormData({ partNumber: '', description: '', customer: '', isActive: true });
      loadParts();
    } catch (error) {
      console.error('Failed to save part:', error);
      alert('Failed to save part');
    }
  };

  const handleEdit = (part: Part) => {
    setEditingPart(part);
    setFormData({
      partNumber: part.partNumber,
      description: part.description,
      customer: part.customer || '',
      isActive: part.isActive,
    });
    setShowForm(true);
  };

  const handleDelete = async (partId: number) => {
    if (!confirm('Are you sure you want to delete this part?')) return;
    
    try {
      await api.delete(`/parts/${partId}`);
      loadParts();
    } catch (error) {
      console.error('Failed to delete part:', error);
    }
  };

  const toggleExpand = async (partId: number) => {
    if (expandedPart === partId) {
      setExpandedPart(null);
    } else {
      setExpandedPart(partId);
      await loadPartDetails(partId);
    }
  };

  const handleUploadClick = (partId: number) => {
    setUploadingPartId(partId);
    setShowUploadForm(true);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setUploadData({ ...uploadData, file: e.target.files[0] });
    }
  };

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!uploadData.operationName || !uploadingPartId) {
      alert('Please fill in all required fields');
      return;
    }

    if (!uploadData.useLinkMode && !uploadData.file) {
      alert('Please select a file to upload');
      return;
    }

    if (uploadData.useLinkMode && !uploadData.filePath) {
      alert('Please enter the file path');
      return;
    }

    const part = parts.find(p => p.partId === uploadingPartId);
    if (!part) return;

    try {
      if (uploadData.useLinkMode) {
        // Link to existing file
        const linkData = {
          filePath: uploadData.filePath,
          partNumber: part.partNumber,
          partDescription: part.description,
          operationName: uploadData.operationName,
          instructions: uploadData.instructions,
          createNewOperation: uploadData.createNewOperation,
          archiveReason: uploadData.archiveReason,
        };

        await api.post('/setupsheets/link', linkData);
      } else {
        // Upload file
        const formDataObj = new FormData();
        formDataObj.append('file', uploadData.file!);
        formDataObj.append('partNumber', part.partNumber);
        formDataObj.append('partDescription', part.description);
        formDataObj.append('operationName', uploadData.operationName);
        formDataObj.append('instructions', uploadData.instructions);
        formDataObj.append('createNewOperation', uploadData.createNewOperation.toString());
        if (uploadData.archiveReason) {
          formDataObj.append('archiveReason', uploadData.archiveReason);
        }

        await api.post('/setupsheets/upload', formDataObj, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
      }

      setShowUploadForm(false);
      setUploadData({
        operationName: '',
        instructions: '',
        createNewOperation: false,
        archiveReason: '',
        file: null,
        filePath: '',
        useLinkMode: false,
      });
      setUploadingPartId(null);
      
      // Reload part details
      await loadPartDetails(uploadingPartId);
    } catch (error) {
      console.error('Failed to upload setup sheet:', error);
      alert('Failed to upload setup sheet');
    }
  };

  const handleArchiveSheet = async (sheetId: number, partId: number) => {
    const reason = prompt('Enter archive reason:');
    if (!reason) return;

    try {
      await api.post(`/setupsheets/${sheetId}/archive`, JSON.stringify(reason), {
        headers: { 'Content-Type': 'application/json' },
      });
      await loadPartDetails(partId);
    } catch (error) {
      console.error('Failed to archive setup sheet:', error);
    }
  };

  const handleDeleteSheet = async (sheetId: number, partId: number) => {
    if (!confirm('Are you sure you want to permanently delete this setup sheet?')) return;

    try {
      await api.delete(`/setupsheets/${sheetId}`);
      await loadPartDetails(partId);
    } catch (error) {
      console.error('Failed to delete setup sheet:', error);
    }
  };

  const handleDownloadSheet = async (sheet: SetupSheet) => {
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
      <div className="px-6 py-4 border-b border-gray-700">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold text-white">Parts Management</h2>
          <button
            onClick={() => setShowForm(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-2"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Add Part
          </button>
        </div>

        {/* Search and Filter */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <input
              type="text"
              placeholder="Search parts..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <select
              value={customerFilter}
              onChange={(e) => setCustomerFilter(e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Customers</option>
              {getUniqueCustomers().map(customer => (
                <option key={customer} value={customer}>{customer}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center">
            <label className="flex items-center gap-2 text-sm text-gray-300">
              <input
                type="checkbox"
                checked={showArchived}
                onChange={(e) => setShowArchived(e.target.checked)}
                className="rounded"
              />
              Show Archived Sheets
            </label>
          </div>
        </div>
      </div>

      {/* Part Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg shadow-xl w-full max-w-md mx-4">
            <div className="px-6 py-4 border-b border-gray-700">
              <h3 className="text-lg font-semibold text-white">
                {editingPart ? 'Edit Part' : 'Add New Part'}
              </h3>
            </div>
            <form onSubmit={handleSubmit} className="px-6 py-4">
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Part Number *
                </label>
                <input
                  type="text"
                  required
                  value={formData.partNumber}
                  onChange={(e) => setFormData({ ...formData, partNumber: e.target.value })}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Description *
                </label>
                <textarea
                  required
                  rows={3}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Customer
                </label>
                <input
                  type="text"
                  value={formData.customer}
                  onChange={(e) => setFormData({ ...formData, customer: e.target.value })}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div className="mb-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    className="mr-2"
                  />
                  <span className="text-sm font-medium text-gray-300">Active</span>
                </label>
              </div>
              <div className="flex gap-2 justify-end">
                <button
                  type="button"
                  onClick={() => {
                    setShowForm(false);
                    setEditingPart(null);
                    setFormData({ partNumber: '', description: '', customer: '', isActive: true });
                  }}
                  className="px-4 py-2 border border-gray-600 text-gray-300 rounded hover:bg-gray-700"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  {editingPart ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Upload Setup Sheet Modal */}
      {showUploadForm && uploadingPartId && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 overflow-y-auto">
          <div className="bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl mx-4 my-8">
            <div className="px-6 py-4 border-b border-gray-700">
              <h3 className="text-lg font-semibold text-white">Upload Setup Sheet</h3>
              <p className="text-sm text-gray-400 mt-1">
                Part: {parts.find(p => p.partId === uploadingPartId)?.partNumber}
              </p>
            </div>
            <form onSubmit={handleUpload} className="px-6 py-4 max-h-[70vh] overflow-y-auto">
              <div className="mb-4">
                <div className="flex gap-4 mb-4">
                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      checked={!uploadData.useLinkMode}
                      onChange={() => setUploadData({ ...uploadData, useLinkMode: false })}
                      className="rounded"
                    />
                    <span className="text-sm text-gray-300">Upload New File</span>
                  </label>
                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      checked={uploadData.useLinkMode}
                      onChange={() => setUploadData({ ...uploadData, useLinkMode: true })}
                      className="rounded"
                    />
                    <span className="text-sm text-gray-300">Link to Existing File</span>
                  </label>
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

              {uploadData.useLinkMode ? (
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    File Path *
                  </label>
                  <input
                    type="text"
                    required
                    value={uploadData.filePath}
                    onChange={(e) => setUploadData({ ...uploadData, filePath: e.target.value })}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="e.g., Z:\\SetupSheets\\2220-0137_MillOpA.xlsx"
                  />
                  <p className="mt-1 text-sm text-gray-400">Enter the full path to the Excel file on the network drive</p>
                </div>
              ) : (
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Excel File *
                  </label>
                  <input
                    type="file"
                    accept=".xlsx,.xls"
                    required={!uploadData.useLinkMode}
                    onChange={handleFileChange}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 text-white rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <p className="mt-1 text-sm text-gray-400">Only .xlsx and .xls files are allowed</p>
                </div>
              )}

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
                      operationName: '',
                      instructions: '',
                      createNewOperation: false,
                      archiveReason: '',
                      file: null,
                      filePath: '',
                      useLinkMode: false,
                    });
                    setUploadingPartId(null);
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
              <ExcelViewer 
                fileUrl={`http://localhost:5000/api/setupsheets/view/${viewingSheet.id}`}
                filePath={viewingSheet.filePath}
              />
            </div>
          </div>
        </div>
      )}

      {/* Parts Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-700">
          <thead className="bg-gray-900">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider w-12"></th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Part Number
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Description
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Customer
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-gray-800 divide-y divide-gray-700">
            {filteredParts.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-4 text-center text-gray-400">
                  No parts found. Click "Add Part" to create one.
                </td>
              </tr>
            ) : (
              filteredParts.map((part) => (
                <React.Fragment key={part.partId}>
                  <tr className="hover:bg-gray-700">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <button
                        onClick={() => toggleExpand(part.partId)}
                        className="text-gray-400 hover:text-white"
                      >
                        <svg
                          className={`w-5 h-5 transform transition-transform ${expandedPart === part.partId ? 'rotate-90' : ''}`}
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                      </button>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap font-medium text-white">{part.partNumber}</td>
                    <td className="px-6 py-4 text-gray-300">{part.description}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-gray-300">{part.customer || '-'}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 py-1 text-xs font-semibold rounded-full ${
                          part.isActive
                            ? 'bg-green-900 text-green-200'
                            : 'bg-red-900 text-red-200'
                        }`}
                      >
                        {part.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button
                        onClick={() => handleUploadClick(part.partId)}
                        className="text-green-400 hover:text-green-300 mr-3"
                      >
                        📄 Upload Sheet
                      </button>
                      <button
                        onClick={() => handleEdit(part)}
                        className="text-blue-400 hover:text-blue-300 mr-3"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(part.partId)}
                        className="text-red-400 hover:text-red-300"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                  {expandedPart === part.partId && (
                    <tr>
                      <td colSpan={6} className="px-6 py-4 bg-gray-900">
                        <div className="pl-12">
                          <h4 className="text-md font-semibold text-white mb-3">Operations & Setup Sheets</h4>
                          {!part.revisions || part.revisions.length === 0 ? (
                            <p className="text-gray-400 text-sm">No operations or setup sheets found for this part.</p>
                          ) : (
                            part.revisions.map(revision => (
                              <div key={revision.partRevisionId} className="mb-4">
                                <div className="text-sm text-gray-400 mb-2">Revision: {revision.revisionNumber}</div>
                                {revision.operations?.map(operation => (
                                  <div key={operation.operationId} className="mb-3 border border-gray-700 rounded p-3">
                                    <div className="font-medium text-white mb-2">
                                      🔧 {operation.operationName}
                                    </div>
                                    {operation.setupSheets && operation.setupSheets.length > 0 ? (
                                      <div className="space-y-2">
                                        {operation.setupSheets
                                          .filter(sheet => showArchived || !sheet.isArchived)
                                          .map(sheet => (
                                          <div key={sheet.id} className="flex items-center justify-between bg-gray-800 p-2 rounded">
                                            <div className="flex-1">
                                              <span className="text-sm text-gray-300">{sheet.description}</span>
                                              <span className="ml-2 text-xs text-gray-500">v{sheet.version}</span>
                                              {sheet.isArchived && (
                                                <span className="ml-2 px-2 py-0.5 text-xs bg-yellow-900 text-yellow-200 rounded">
                                                  Archived
                                                </span>
                                              )}
                                            </div>
                                            <div className="flex gap-2">
                                              <button
                                                onClick={() => setViewingSheet(sheet)}
                                                className="text-blue-400 hover:text-blue-300 text-xs"
                                              >
                                                View
                                              </button>
                                              <button
                                                onClick={() => handleDownloadSheet(sheet)}
                                                className="text-green-400 hover:text-green-300 text-xs"
                                              >
                                                Download
                                              </button>
                                              {!sheet.isArchived && (
                                                <button
                                                  onClick={() => handleArchiveSheet(sheet.id, part.partId)}
                                                  className="text-yellow-400 hover:text-yellow-300 text-xs"
                                                >
                                                  Archive
                                                </button>
                                              )}
                                              <button
                                                onClick={() => handleDeleteSheet(sheet.id, part.partId)}
                                                className="text-red-400 hover:text-red-300 text-xs"
                                              >
                                                Delete
                                              </button>
                                            </div>
                                          </div>
                                        ))}
                                      </div>
                                    ) : (
                                      <p className="text-gray-500 text-xs">No setup sheets for this operation</p>
                                    )}
                                  </div>
                                ))}
                              </div>
                            ))
                          )}
                        </div>
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
