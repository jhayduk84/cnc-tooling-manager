import axios from 'axios';
import type {
  Part,
  Operation,
  SetupSheet,
  ToolAssemblyAvailability,
  ToolComponent,
  ToolAssembly,
  Machine,
  InventoryLocation
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Operator API
export const operatorApi = {
  getPartByNumber: async (partNumber: string): Promise<Part> => {
    const response = await api.get(`/parts/${partNumber}`);
    return response.data;
  },

  getPartOperations: async (partNumber: string, revision?: string): Promise<Operation[]> => {
    const response = await api.get(`/parts/${partNumber}/operations`, {
      params: { revision }
    });
    return response.data;
  },

  getOperation: async (operationId: number): Promise<Operation> => {
    const response = await api.get(`/operations/${operationId}`);
    return response.data;
  },

  getSetupSheet: async (operationId: number): Promise<SetupSheet> => {
    const response = await api.get(`/operations/${operationId}/setup-sheet`);
    return response.data;
  },

  getOperationTooling: async (operationId: number, includeLocations = false): Promise<ToolAssemblyAvailability[]> => {
    const endpoint = includeLocations 
      ? `/operations/${operationId}/tooling/with-locations`
      : `/operations/${operationId}/tooling`;
    const response = await api.get(endpoint);
    return response.data;
  },
};

// Admin API - Tool Components
export const toolComponentApi = {
  getAll: async (activeOnly = true): Promise<ToolComponent[]> => {
    const response = await api.get('/tool-components', { params: { activeOnly } });
    return response.data;
  },

  getById: async (id: number): Promise<ToolComponent> => {
    const response = await api.get(`/tool-components/${id}`);
    return response.data;
  },

  create: async (component: Partial<ToolComponent>): Promise<ToolComponent> => {
    const response = await api.post('/tool-components', component);
    return response.data;
  },

  update: async (id: number, component: Partial<ToolComponent>): Promise<ToolComponent> => {
    const response = await api.put(`/tool-components/${id}`, component);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/tool-components/${id}`);
  },
};

// Admin API - Tool Assemblies
export const toolAssemblyApi = {
  getAll: async (activeOnly = true): Promise<ToolAssembly[]> => {
    const response = await api.get('/tool-assemblies', { params: { activeOnly } });
    return response.data;
  },

  getById: async (id: number): Promise<ToolAssembly> => {
    const response = await api.get(`/tool-assemblies/${id}`);
    return response.data;
  },

  create: async (assembly: Partial<ToolAssembly>): Promise<ToolAssembly> => {
    const response = await api.post('/tool-assemblies', assembly);
    return response.data;
  },

  update: async (id: number, assembly: Partial<ToolAssembly>): Promise<ToolAssembly> => {
    const response = await api.put(`/tool-assemblies/${id}`, assembly);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/tool-assemblies/${id}`);
  },
};

// Admin API - Machines
export const machineApi = {
  getAll: async (activeOnly = true): Promise<Machine[]> => {
    const response = await api.get('/machines', { params: { activeOnly } });
    return response.data;
  },

  getById: async (id: number): Promise<Machine> => {
    const response = await api.get(`/machines/${id}`);
    return response.data;
  },

  create: async (machine: Partial<Machine>): Promise<Machine> => {
    const response = await api.post('/machines', machine);
    return response.data;
  },

  update: async (id: number, machine: Partial<Machine>): Promise<Machine> => {
    const response = await api.put(`/machines/${id}`, machine);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/machines/${id}`);
  },
};

// Admin API - Inventory Locations
export const inventoryLocationApi = {
  getAll: async (activeOnly = true): Promise<InventoryLocation[]> => {
    const response = await api.get('/inventory-locations', { params: { activeOnly } });
    return response.data;
  },

  getById: async (id: number): Promise<InventoryLocation> => {
    const response = await api.get(`/inventory-locations/${id}`);
    return response.data;
  },

  create: async (location: Partial<InventoryLocation>): Promise<InventoryLocation> => {
    const response = await api.post('/inventory-locations', location);
    return response.data;
  },

  update: async (id: number, location: Partial<InventoryLocation>): Promise<InventoryLocation> => {
    const response = await api.put(`/inventory-locations/${id}`, location);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/inventory-locations/${id}`);
  },
};

export default api;
