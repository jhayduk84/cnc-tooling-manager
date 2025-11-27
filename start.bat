@echo off
echo ==========================================
echo CNC Tooling Manager - Setup Script
echo ==========================================
echo.

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo X Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

echo + Docker is running
echo.
echo Starting CNC Tooling Manager with Docker...
echo.

REM Build and start containers
docker-compose up --build -d

echo.
echo ==========================================
echo + CNC Tooling Manager is starting!
echo ==========================================
echo.
echo Services:
echo   - Frontend:  http://localhost:5173
echo   - API:       http://localhost:5000
echo   - Swagger:   http://localhost:5000/swagger
echo   - Database:  localhost:1433
echo.
echo Sample Part Numbers for Testing:
echo   - 12345-01 (Mounting Bracket - 2 operations)
echo   - 67890-02 (Housing Cover - 1 operation)
echo.
echo View logs:
echo   docker-compose logs -f
echo.
echo Stop services:
echo   docker-compose down
echo.
echo Note: First startup may take a few minutes while
echo       the database initializes and seeds data.
echo ==========================================
echo.
pause
