#!/bin/bash

echo "Waiting for SQL Server to start..."
sleep 30

echo "Running database initialization scripts..."

# Run each SQL script in order
for script in /opt/mssql-init/*.sql; do
    echo "Running $script..."
    /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -i "$script"
done

echo "Database initialization complete!"