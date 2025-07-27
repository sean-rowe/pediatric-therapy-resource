#!/bin/bash
# Run SQL migrations for TherapyDocs

set -e

# Configuration
DB_SERVER="${DB_SERVER:-localhost,1433}"
DB_NAME="${DB_NAME:-TherapyDocs}"
DB_USER="${DB_USER:-sa}"
DB_PASSWORD="${DB_PASSWORD:-Th3r@pyD0cs2024!}"

MIGRATION_DIR="$(dirname "$0")/../sql/migrations"

echo "🏥 TherapyDocs Database Migration Runner"
echo "======================================="
echo "Server: $DB_SERVER"
echo "Database: $DB_NAME"
echo ""

# Check if sqlcmd is available
if ! command -v sqlcmd &> /dev/null; then
    echo "❌ Error: sqlcmd is not installed. Please install SQL Server command line tools."
    exit 1
fi

# Check if migration directory exists
if [ ! -d "$MIGRATION_DIR" ]; then
    echo "❌ Error: Migration directory not found: $MIGRATION_DIR"
    exit 1
fi

# Run migrations in order
echo "📋 Running migrations..."
for migration in "$MIGRATION_DIR"/*.sql; do
    if [ -f "$migration" ]; then
        filename=$(basename "$migration")
        echo ""
        echo "🔄 Running: $filename"
        
        if sqlcmd -S "$DB_SERVER" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" -i "$migration" -b; then
            echo "✅ Success: $filename"
        else
            echo "❌ Failed: $filename"
            exit 1
        fi
    fi
done

echo ""
echo "🎉 All migrations completed successfully!"
echo ""

# Optional: Display current schema info
echo "📊 Current schema information:"
sqlcmd -S "$DB_SERVER" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" -Q "
SELECT 
    'Tables' as ObjectType, 
    COUNT(*) as Count 
FROM sys.tables
UNION ALL
SELECT 
    'Stored Procedures' as ObjectType, 
    COUNT(*) as Count 
FROM sys.procedures
UNION ALL
SELECT 
    'Indexes' as ObjectType, 
    COUNT(*) as Count 
FROM sys.indexes 
WHERE object_id IN (SELECT object_id FROM sys.tables)
    AND name IS NOT NULL
" -W -s"|"