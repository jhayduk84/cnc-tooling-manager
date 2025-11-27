# CNC Tooling Manager

A comprehensive web application for managing CNC milling department tooling, holders, and setup information. Built for shop floor kiosk operations with barcode scanning support.

## 🎯 Features

### Operator Interface
- **Barcode Scanning**: Scan part numbers to instantly retrieve setup information
- **Setup Sheet Display**: View PDF or HTML setup sheets with print functionality
- **Tool Availability**: Real-time visibility of tool assembly availability
- **Location Tracking**: Toggle detailed view to see where each component is located
- **Kiosk-Optimized UI**: Large fonts and simple navigation for shop floor use

### Admin Interface
- **Tool Components Management**: Cutters, holders, collets, extensions, inserts
- **Tool Assemblies**: Manage complete tool setups with component lists
- **Machine Management**: Track machines and tool pocket assignments
- **Inventory Locations**: Manage crib locations and storage areas
- **Parts & Operations**: Part revisions and operation sequences
- **Setup Kits**: Pre-staged tooling for upcoming jobs

### Technical Features
- Real-time inventory status tracking
- Component location tracking (crib, machine, setup kit, regrind, etc.)
- Availability calculation (Fully Available, Partially Available, Not Available)
- Esprit KBM integration ready (foreign key fields in place)
- SQL Server database with EF Core
- REST API with comprehensive endpoints
- Docker containerization

## 🏗️ Architecture

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022 / SQL Server Express
- **Architecture**: Clean/Layered (Domain, Infrastructure, Application, API)

### Frontend
- **Framework**: React 18 + TypeScript
- **Build Tool**: Vite
- **UI**: Tailwind CSS
- **Routing**: React Router v6
- **HTTP Client**: Axios

### Project Structure
```
cnc-tooling-manager/
├── backend/
│   ├── CncTooling.Api/              # Web API controllers
│   ├── CncTooling.Application/      # Business logic, DTOs, services
│   ├── CncTooling.Domain/           # Domain entities
│   ├── CncTooling.Infrastructure/   # EF Core, DbContext, migrations
│   └── CncTooling.sln
├── frontend/
│   ├── src/
│   │   ├── pages/                   # React pages
│   │   ├── components/              # Reusable components
│   │   ├── services/                # API client
│   │   └── types/                   # TypeScript types
│   └── package.json
├── docker-compose.yml
├── ARCHITECTURE.md
└── README.md
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- SQL Server 2022 or SQL Server Express (for local dev without Docker)
- Docker & Docker Compose (for containerized deployment)

### Option 1: Run with Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd cnc-tooling-manager
   ```

2. **Start all services**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - Frontend: http://localhost:5173
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger

The database will be automatically created and seeded with sample data on first run.

### Option 2: Run Locally (Development)

#### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd backend
   ```

2. **Update connection string** (if needed)
   
   Edit `CncTooling.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=CncToolingDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
   }
   ```

3. **Create and apply migrations**
   ```bash
   cd CncTooling.Api
   dotnet ef migrations add InitialCreate --project ../CncTooling.Infrastructure --startup-project .
   dotnet ef database update
   ```

   Or use the provided script:
   ```bash
   chmod +x create-migration.sh
   ./create-migration.sh
   cd CncTooling.Api
   dotnet ef database update
   ```

4. **Run the API**
   ```bash
   cd CncTooling.Api
   dotnet run
   ```

   API will be available at: http://localhost:5000

#### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Update API URL** (if needed)
   
   Edit `frontend/.env`:
   ```
   VITE_API_URL=http://localhost:5000/api
   ```

4. **Run the development server**
   ```bash
   npm run dev
   ```

   Frontend will be available at: http://localhost:5173

## 📊 Database Schema

### Core Entities

**Parts & Operations**
- `Parts` - Part master records
- `PartRevisions` - Part revisions (A, B, C, etc.)
- `Operations` - Operations for each revision (OP10, OP20, etc.)
- `SetupSheets` - Setup sheet files/URLs

**Tooling**
- `ToolComponents` - Individual components (cutters, holders, collets, etc.)
- `ToolAssemblies` - Complete tool setups
- `ToolAssemblyComponents` - Components in each assembly (many-to-many)
- `OperationToolAssemblies` - Tools required for operations (many-to-many)

**Inventory & Locations**
- `InventoryLocations` - Crib locations, racks, etc.
- `ComponentInventoryStatus` - Current location and status of each component
- `Machines` - CNC machines
- `MachineToolLocations` - Tool pockets/stations in machines

**Setup Kits**
- `SetupKits` - Pre-staged tooling kits
- `SetupKitItems` - Items in each kit

See `ARCHITECTURE.md` for detailed schema and relationships.

## 🔌 API Endpoints

### Operator Flow
```
GET  /api/parts/{partNumber}                         # Get part with operations
GET  /api/parts/{partNumber}/operations              # List operations
GET  /api/operations/{id}                            # Get operation details
GET  /api/operations/{id}/setup-sheet                # Get setup sheet
GET  /api/operations/{id}/tooling                    # Get tool availability
GET  /api/operations/{id}/tooling/with-locations     # Get tools with locations
```

### Admin CRUD
```
GET/POST/PUT/DELETE  /api/tool-components
GET/POST/PUT/DELETE  /api/tool-assemblies
GET/POST/PUT/DELETE  /api/machines
GET/POST/PUT/DELETE  /api/inventory-locations
```

Full API documentation available at: http://localhost:5000/swagger

## 💡 Usage

### Operator Workflow

1. **Scan Part Number**
   - Operator scans barcode containing part number
   - System looks up part and displays active operations

2. **Select Operation**
   - If multiple operations exist, operator selects one (OP10, OP20, etc.)
   - If only one operation, auto-navigates to operation view

3. **View Setup & Tooling**
   - Left panel: Setup sheet with Print button
   - Right panel: Required tool assemblies with availability status
   - Toggle "Show Locations" to see detailed component locations

4. **Check Availability**
   - **Green** = Fully Available (all components ready)
   - **Yellow** = Partially Available (some components missing)
   - **Red** = Not Available (critical components missing)

### Admin Workflow

1. **Access Admin Panel**
   - Click "Admin Panel" link on scan page
   - Or navigate to `/admin`

2. **Manage Tool Components**
   - Add cutters, holders, collets, extensions, inserts
   - Track asset tags for high-value items
   - Set manufacturer, cost, and notes

3. **Build Tool Assemblies**
   - Create assemblies (e.g., "T1 - 1/2" Rougher")
   - Add components to assemblies
   - Mark primary components (main cutter vs. consumables)

4. **Track Inventory**
   - Update component locations
   - Track status (Available, InMachine, OutForRegrind, Scrap, etc.)
   - Monitor quantities on hand

## 🔧 Configuration

### Environment Variables

**Backend** (`backend/CncTooling.Api/appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db,1433;Database=CncToolingDb;..."
  }
}
```

**Frontend** (`frontend/.env`)
```
VITE_API_URL=http://localhost:5000/api
```

### Database Connection

- **Docker**: Connection string points to `db` service (SQL Server container)
- **Local**: Update to point to your local SQL Server instance

## 📦 Sample Data

The application includes a database seeder that creates sample data on first run:

- 2 Parts with revisions
- 3 Operations with setup sheets
- 8 Tool components (cutters, holders, collets)
- 2 Tool assemblies
- 3 Machines
- 5 Inventory locations
- Component inventory statuses
- 1 Setup kit

## 🔮 Esprit KBM Integration (Future)

The system is designed to integrate with Esprit KBM (SQL Server Express):

**Integration Points:**
- `EspritToolId` fields in `ToolComponent` and `ToolAssembly`
- `EspritProgramName` and `EspritProgramId` in `Operation`
- Placeholder service: `Services/EspritIntegrationService.cs`

**Integration Strategy:**
1. Query Esprit schema for tool and program data
2. Import/sync tools and programs into CNC Tooling Manager
3. Maintain foreign key references
4. Keep data synchronized via scheduled jobs or triggers

## 🛠️ Development

### Adding a New Entity

1. Create entity class in `backend/CncTooling.Domain/Entities/`
2. Add DbSet to `CncToolingDbContext.cs`
3. Configure entity in `OnModelCreating()`
4. Create migration: `dotnet ef migrations add AddNewEntity`
5. Create DTO in `backend/CncTooling.Application/DTOs/`
6. Create service methods if needed
7. Create controller in `backend/CncTooling.Api/Controllers/`
8. Add TypeScript types in `frontend/src/types/`
9. Add API methods in `frontend/src/services/api.ts`
10. Create admin UI page

### Running Tests

```bash
# Backend (when tests are added)
cd backend
dotnet test

# Frontend (when tests are added)
cd frontend
npm test
```

## 📝 License

[Your License Here]

## 🤝 Contributing

[Your Contributing Guidelines Here]

## 📧 Support

For questions or issues, please [contact/create an issue].

---

**Built for CNC milling departments with ❤️**