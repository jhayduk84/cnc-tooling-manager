#!/bin/bash
# Script to create initial EF Core migration

cd "$(dirname "$0")"
cd CncTooling.Api

echo "Creating initial migration..."
dotnet ef migrations add InitialCreate --project ../CncTooling.Infrastructure --startup-project . --context CncToolingDbContext

echo "Migration created successfully!"
echo "To apply migration, run: dotnet ef database update"
