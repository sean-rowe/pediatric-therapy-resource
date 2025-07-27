#!/bin/bash

echo "Starting TherapyDocs MSSQL Database..."

# Copy .env.example to .env if it doesn't exist
if [ ! -f .env ]; then
    echo "Creating .env file from .env.example..."
    cp .env.example .env
fi

# Start the database
docker-compose up -d mssql

echo "Waiting for MSSQL to be ready..."
sleep 30

# Run initialization if this is the first time
if docker-compose exec mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "${MSSQL_SA_PASSWORD:-TherapyDocs2024!}" -Q "SELECT name FROM sys.databases WHERE name = 'TherapyDocs'" | grep -q "TherapyDocs"; then
    echo "Database already initialized."
else
    echo "Initializing database..."
    docker-compose exec mssql /bin/bash /opt/mssql-init/../init-db.sh
fi

echo "TherapyDocs database is ready!"
echo "Connection string: Server=localhost,1433;Database=TherapyDocs;User Id=SA;Password=TherapyDocs2024!;Encrypt=false;TrustServerCertificate=true"