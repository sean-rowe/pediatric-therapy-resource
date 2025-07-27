#!/bin/bash
# Database migration runner script for UPTRMS
# Runs initialization scripts and migrations in order

set -euo pipefail

# Configuration
DB_SERVER="${DB_SERVER:-localhost}"
DB_PORT="${DB_PORT:-1433}"
DB_USER="${DB_USER:-sa}"
DB_PASSWORD="${DB_PASSWORD}"
DB_NAME="${DB_NAME:-UPTRMS}"
MIGRATION_TABLE="_MigrationHistory"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Wait for SQL Server to be ready
wait_for_sqlserver() {
    log_info "Waiting for SQL Server to be ready..."
    local max_attempts=60
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -Q "SELECT 1" &>/dev/null; then
            log_info "SQL Server is ready!"
            return 0
        fi
        
        log_info "Attempt $attempt/$max_attempts: SQL Server not ready yet..."
        sleep 5
        attempt=$((attempt + 1))
    done
    
    log_error "SQL Server did not become ready in time"
    return 1
}

# Check if database exists
database_exists() {
    local result=$(/opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" \
        -h -1 -W -Q "SELECT COUNT(*) FROM sys.databases WHERE name = '$DB_NAME'")
    
    if [ "$result" -eq "1" ]; then
        return 0
    else
        return 1
    fi
}

# Create migration history table
create_migration_table() {
    log_info "Creating migration history table..."
    /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" <<EOF
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '$MIGRATION_TABLE')
BEGIN
    CREATE TABLE $MIGRATION_TABLE (
        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL,
        AppliedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Checksum NVARCHAR(64) NULL,
        AppliedBy NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER
    );
    
    CREATE INDEX IX_MigrationHistory_AppliedOn ON $MIGRATION_TABLE (AppliedOn DESC);
END
EOF
}

# Check if migration has been applied
migration_applied() {
    local migration_id=$1
    local result=$(/opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" \
        -h -1 -W -Q "SELECT COUNT(*) FROM $MIGRATION_TABLE WHERE MigrationId = '$migration_id'")
    
    if [ "$result" -eq "1" ]; then
        return 0
    else
        return 1
    fi
}

# Calculate file checksum
calculate_checksum() {
    local file=$1
    sha256sum "$file" | cut -d' ' -f1
}

# Record migration
record_migration() {
    local migration_id=$1
    local checksum=$2
    local version="${VERSION:-1.0.0}"
    
    /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" <<EOF
INSERT INTO $MIGRATION_TABLE (MigrationId, ProductVersion, Checksum)
VALUES ('$migration_id', '$version', '$checksum');
EOF
}

# Run a SQL file
run_sql_file() {
    local file=$1
    local description=$2
    
    log_info "Running: $description"
    
    # Use -I to enable QUOTED_IDENTIFIER for MSSQL compliance
    if /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" -I -i "$file"; then
        log_info "Successfully executed: $description"
        return 0
    else
        log_error "Failed to execute: $description"
        return 1
    fi
}

# Main migration process
main() {
    log_info "Starting database migration process..."
    
    # Wait for SQL Server
    if ! wait_for_sqlserver; then
        exit 1
    fi
    
    # Run initialization scripts (these run against master database)
    if ! database_exists; then
        log_info "Database does not exist. Running initialization scripts..."
        
        for script in init-scripts/*.sql; do
            if [ -f "$script" ]; then
                filename=$(basename "$script")
                log_info "Running initialization script: $filename"
                
                # Init scripts run against master database
                if /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -I -i "$script"; then
                    log_info "Successfully executed: $filename"
                else
                    log_error "Failed to execute: $filename"
                    exit 1
                fi
            fi
        done
    else
        log_info "Database already exists. Skipping initialization scripts."
    fi
    
    # Create migration table
    create_migration_table
    
    # Run migrations in order
    log_info "Running migrations..."
    migration_count=0
    skipped_count=0
    
    for migration in migrations/*.sql; do
        if [ -f "$migration" ]; then
            filename=$(basename "$migration")
            migration_id="${filename%.sql}"
            checksum=$(calculate_checksum "$migration")
            
            if migration_applied "$migration_id"; then
                log_warn "Migration already applied: $migration_id"
                skipped_count=$((skipped_count + 1))
            else
                if run_sql_file "$migration" "$migration_id"; then
                    record_migration "$migration_id" "$checksum"
                    migration_count=$((migration_count + 1))
                else
                    log_error "Migration failed. Stopping migration process."
                    exit 1
                fi
            fi
        fi
    done
    
    log_info "Migration complete! Applied: $migration_count, Skipped: $skipped_count"
    
    # Run post-migration validation
    log_info "Running post-migration validation..."
    /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER,$DB_PORT" -U "$DB_USER" -P "$DB_PASSWORD" -d "$DB_NAME" <<EOF
-- Verify critical tables exist
DECLARE @missing_tables NVARCHAR(MAX) = '';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    SET @missing_tables = @missing_tables + 'Users, ';
    
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resources')
    SET @missing_tables = @missing_tables + 'Resources, ';
    
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
    SET @missing_tables = @missing_tables + 'Sessions, ';

IF LEN(@missing_tables) > 0
BEGIN
    RAISERROR('Missing critical tables: %s', 16, 1, @missing_tables);
END
ELSE
BEGIN
    PRINT 'All critical tables verified successfully';
END

-- Verify encryption
IF EXISTS (
    SELECT 1 FROM sys.databases 
    WHERE name = '$DB_NAME' 
    AND is_encrypted = 0
)
BEGIN
    PRINT 'WARNING: Database is not encrypted. Enable TDE for HIPAA compliance.';
END
ELSE
BEGIN
    PRINT 'Database encryption verified';
END
EOF
    
    log_info "Database migration process completed successfully!"
}

# Run main function
main "$@"