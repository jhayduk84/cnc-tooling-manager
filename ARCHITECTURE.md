# CNC Tooling Manager - Architecture

## Solution Structure

```
cnc-tooling-manager/
├── backend/                          # ASP.NET Core solution
│   ├── CncTooling.Api/              # Web API project
│   ├── CncTooling.Application/      # Application/business logic
│   ├── CncTooling.Domain/           # Domain entities and interfaces
│   ├── CncTooling.Infrastructure/   # Data access, EF Core
│   └── CncTooling.sln
├── frontend/                         # React + TypeScript app
│   ├── src/
│   │   ├── components/              # Reusable components
│   │   ├── pages/                   # Route pages
│   │   ├── services/                # API client services
│   │   ├── types/                   # TypeScript interfaces
│   │   └── App.tsx
│   ├── package.json
│   └── vite.config.ts
├── docker-compose.yml
└── README.md
```

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022 / SQL Server Express
- **Architecture**: Clean/Layered Architecture
  - API Layer: Controllers, middleware
  - Application Layer: Services, DTOs, business logic
  - Domain Layer: Entities, interfaces
  - Infrastructure Layer: EF Core, repositories

### Frontend
- **Framework**: React 18 + TypeScript
- **Build Tool**: Vite
- **UI Framework**: Tailwind CSS
- **Routing**: React Router v6
- **HTTP Client**: Axios
- **State Management**: React Context (simple) or Zustand (if needed)

### DevOps
- **Containerization**: Docker & Docker Compose
- **Database Migrations**: EF Core Migrations

## Database Schema Overview

### Core Entities

**Parts** ← **PartRevisions** ← **Operations** ← **SetupSheets**
- Parts have multiple revisions
- Each revision has multiple operations (OP10, OP20, etc.)
- Each operation references a setup sheet

**ToolComponents** ↔ **ToolAssemblies** (many-to-many via **ToolAssemblyComponents**)
- Components: cutters, holders, collets, extensions, inserts
- Assemblies: complete tool setups (T1, T2, etc.)

**Machines** ← **MachineToolLocations**
- Track which assemblies/components are in which machine pocket

**SetupKits** ← **SetupKitItems**
- Pre-staged tooling for upcoming jobs

**InventoryLocations** + **ComponentInventoryStatus**
- Track where components are: crib, machine, kit, regrind, scrap

### Relationships
- Operations → ToolAssemblies (many-to-many): what tools are required for an operation
- ComponentInventoryStatus: tracks current location and status of each component instance

## API Endpoints

### Operator Flow
- `GET /api/parts/{partNumber}` - Get part with active revision
- `GET /api/parts/{partNumber}/operations` - List operations
- `GET /api/operations/{operationId}` - Get operation details
- `GET /api/operations/{operationId}/setup-sheet` - Get setup sheet
- `GET /api/operations/{operationId}/tooling` - Get required tools with availability
- `GET /api/operations/{operationId}/tooling/with-locations` - Get tools with detailed locations

### Admin CRUD
- `/api/parts`, `/api/part-revisions`
- `/api/operations`, `/api/setup-sheets`
- `/api/tool-components`, `/api/tool-assemblies`
- `/api/machines`, `/api/machine-tool-locations`
- `/api/inventory-locations`
- `/api/setup-kits`

## Frontend Routes

### Operator Interface
- `/` or `/scan` - Barcode scanning interface
- `/operation/:id` - Operation details with setup sheet and tooling

### Admin Interface
- `/admin/parts` - Parts and revisions management
- `/admin/operations` - Operations and setup sheets
- `/admin/tools` - Tool components and assemblies
- `/admin/machines` - Machines and tool locations
- `/admin/inventory` - Inventory locations
- `/admin/kits` - Setup kits management

## Key Features

### Barcode Scanning Flow
1. Operator scans barcode (keyboard wedge → part number)
2. System looks up part and active revision
3. Shows available operations (if multiple)
4. Displays setup sheet (PDF/HTML) with print button
5. Shows tool assemblies with availability status
6. Toggle to show detailed component locations

### Availability Logic
- **Fully Available**: All required components are available
- **Partially Available**: Some components missing
- **Not Available**: Critical components missing

### Component Location Tracking
- In crib (specific bin/location)
- In machine (machine name + pocket number)
- In setup kit (kit name)
- Out for regrind, scrap, lost, etc.

## Future: Esprit KBM Integration

- Esprit data lives in separate SQL Server schema/database
- Reference fields: `EspritToolId`, `EspritProgramName`, etc.
- Stub service: `EspritIntegrationService` for future sync/query logic
- Can import/sync tool data, program data from Esprit

## Development Setup

1. **Local Development** (no Docker):
   - SQL Server Express on Windows
   - Run API: `dotnet run` from Api project
   - Run frontend: `npm run dev` from frontend folder

2. **Docker Development**:
   - `docker-compose up` - starts all services
   - API on port 5000
   - Frontend on port 5173
   - SQL Server on port 1433

## Assumptions & Design Decisions

1. **Barcode Format**: Simple part number, or `PARTNUM|REV` format
2. **Active Revision**: Each part has a default/active revision
3. **Operation Selection**: Auto-select if only one active operation
4. **Setup Sheets**: Stored as file paths or URLs, not in DB
5. **Component Tracking**: Asset tags for holders/cutters, optional for cheap consumables
6. **Availability**: Based on real-time inventory status queries
7. **Kiosk Mode**: Large fonts, minimal clicks, optimized for touchscreen
8. **Authentication**: Not implemented in v1 (shop floor kiosk scenario)
