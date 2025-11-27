# Quick Start Scripts

## Run with Docker

```bash
# Start all services (API + Frontend + Database)
docker-compose up --build

# Stop all services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

## Run Locally

### Backend
```bash
cd backend/CncTooling.Api
dotnet restore
dotnet ef database update --project ../CncTooling.Infrastructure
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Create Database Migration

```bash
cd backend
chmod +x create-migration.sh
./create-migration.sh

# Or manually:
cd CncTooling.Api
dotnet ef migrations add MigrationName --project ../CncTooling.Infrastructure
dotnet ef database update
```

## Access Points

- **Frontend**: http://localhost:5173
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Database**: localhost:1433 (SQL Server)
  - User: sa
  - Password: YourStrong@Passw0rd

## Sample Part Numbers for Testing

Try scanning these part numbers on the operator interface:
- `12345-01` - Mounting Bracket (2 operations)
- `67890-02` - Housing Cover (1 operation)

## Barcode Format

The system accepts two barcode formats:
- Simple: `PARTNUM` (e.g., `12345-01`)
- With Revision: `PARTNUM|REV` (e.g., `12345-01|A`)

## Troubleshooting

### Database Connection Issues
If the API can't connect to the database:
1. Ensure SQL Server is running
2. Check connection string in `appsettings.json`
3. Verify SQL Server allows remote connections

### Frontend API Connection Issues
If the frontend can't reach the API:
1. Verify API is running on port 5000
2. Check CORS settings in API `Program.cs`
3. Update `VITE_API_URL` in frontend `.env`

### Docker Issues
```bash
# View logs
docker-compose logs api
docker-compose logs frontend
docker-compose logs db

# Rebuild specific service
docker-compose up --build api

# Clean restart
docker-compose down -v
docker system prune -a
docker-compose up --build
```
