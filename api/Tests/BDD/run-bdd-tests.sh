#!/bin/bash

# Run BDD tests with proper configuration
cd "$(dirname "$0")"/../..

echo "Running BDD tests..."

# Ensure test database is running
if ! docker ps | grep -q "mssql"; then
    echo "Starting test database..."
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=TherapyDocs2024!" \
        -p 1433:1433 --name mssql-test -d \
        mcr.microsoft.com/mssql/server:2022-latest
    
    # Wait for database to be ready
    sleep 10
fi

# Run migrations on test database
echo "Running migrations..."
dotnet ef database update --project ../TherapyDocs.Api.csproj --startup-project ../TherapyDocs.Api.csproj

# Run the BDD tests
echo "Executing BDD tests..."
dotnet test --filter "Category=BDD|FullyQualifiedName~BDD" --logger "console;verbosity=detailed"

# Generate living documentation
echo "Generating living documentation..."
livingdoc test-assembly bin/Debug/net8.0/TherapyDocs.Api.Tests.dll -t bin/Debug/net8.0/TestExecution.json

echo "BDD tests completed!"