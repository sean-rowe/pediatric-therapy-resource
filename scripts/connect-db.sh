#!/bin/bash

echo "Connecting to TherapyDocs database..."
docker-compose exec mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "${MSSQL_SA_PASSWORD:-TherapyDocs2024!}" -d TherapyDocs