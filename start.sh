#!/bin/bash

echo "=========================================="
echo "CNC Tooling Manager - Setup Script"
echo "=========================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "   Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    echo "   Visit: https://docs.docker.com/compose/install/"
    exit 1
fi

echo "✅ Docker and Docker Compose are installed"
echo ""
echo "Starting CNC Tooling Manager with Docker..."
echo ""

# Build and start containers
docker-compose up --build -d

echo ""
echo "=========================================="
echo "✅ CNC Tooling Manager is starting!"
echo "=========================================="
echo ""
echo "Services:"
echo "  - Frontend:  http://localhost:5173"
echo "  - API:       http://localhost:5000"
echo "  - Swagger:   http://localhost:5000/swagger"
echo "  - Database:  localhost:1433"
echo ""
echo "Sample Part Numbers for Testing:"
echo "  - 12345-01 (Mounting Bracket - 2 operations)"
echo "  - 67890-02 (Housing Cover - 1 operation)"
echo ""
echo "View logs:"
echo "  docker-compose logs -f"
echo ""
echo "Stop services:"
echo "  docker-compose down"
echo ""
echo "Note: First startup may take a few minutes while"
echo "      the database initializes and seeds data."
echo "=========================================="
