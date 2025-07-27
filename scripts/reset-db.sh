#!/bin/bash

echo "WARNING: This will delete all data in the TherapyDocs database!"
read -p "Are you sure you want to continue? (y/N) " -n 1 -r
echo

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Stopping containers..."
    docker-compose down
    
    echo "Removing volumes..."
    docker volume rm therapy-docs_mssql-data 2>/dev/null || true
    
    echo "Starting fresh database..."
    ./scripts/start-db.sh
    
    echo "Database has been reset!"
else
    echo "Reset cancelled."
fi