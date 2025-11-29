export interface Part {
  partId: number;
  partNumber: string;
  description: string;
  defaultRevisionId?: number;
  isActive: boolean;
  defaultRevision?: PartRevision;
  revisions: PartRevision[];
}

export interface PartRevision {
  partRevisionId: number;
  partId: number;
  revisionCode: string;
  isActive: boolean;
  notes?: string;
  operations: Operation[];
}

export interface Operation {
  operationId: number;
  partRevisionId: number;
  operationName: string;
  espritProgramName?: string;
  espritProgramId?: string;
  setupSheetPath?: string;
  setupSheetUrl?: string;
  isActive: boolean;
  sequenceNumber: number;
  notes?: string;
}

export interface SetupSheet {
  id: number;
  setupSheetId: number;
  operationId: number;
  filePath?: string;
  url?: string;
  format: string;
  description?: string;
  isActive: boolean;
}

export interface ToolComponent {
  toolComponentId: number;
  componentType: string;
  componentCode: string;
  description: string;
  manufacturer?: string;
  assetTag?: string;
  unitCost?: number;
  isActive: boolean;
  espritToolId?: string;
  notes?: string;
}

export interface ToolAssembly {
  toolAssemblyId: number;
  assemblyName: string;
  description: string;
  espritToolId?: string;
  toolNumber?: string;
  isActive: boolean;
  notes?: string;
  components: ToolAssemblyComponent[];
}

export interface ToolAssemblyComponent {
  toolAssemblyComponentId: number;
  toolAssemblyId: number;
  toolComponentId: number;
  quantityRequired: number;
  isPrimary: boolean;
  notes?: string;
  component?: ToolComponent;
}

export interface ToolAssemblyAvailability {
  assembly: ToolAssembly;
  availabilityStatus: string;
  quantityRequired: number;
  componentAvailability: ComponentAvailability[];
}

export interface ComponentAvailability {
  component: ToolComponent;
  quantityRequired: number;
  quantityAvailable: number;
  isPrimary: boolean;
  locations: ComponentLocation[];
}

export interface ComponentLocation {
  locationType: string;
  locationDescription: string;
  status: string;
  quantity: number;
  machineInfo?: string;
  pocketNumber?: number;
}

export interface Machine {
  machineId: number;
  name: string;
  description?: string;
  machineType: string;
  manufacturer?: string;
  model?: string;
  isActive: boolean;
}

export interface InventoryLocation {
  inventoryLocationId: number;
  locationCode: string;
  description: string;
  locationType: string;
  isActive: boolean;
  notes?: string;
}
