#!/bin/bash
# TherapyDocs HIPAA Compliance Audit Script
# This script verifies that all HIPAA compliance requirements are met

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Counters
PASSED=0
FAILED=0
WARNINGS=0

# Helper functions
check_passed() {
    echo -e "${GREEN}✓ PASS${NC} $1"
    ((PASSED++))
}

check_failed() {
    echo -e "${RED}✗ FAIL${NC} $1"
    ((FAILED++))
}

check_warning() {
    echo -e "${YELLOW}⚠ WARN${NC} $1"
    ((WARNINGS++))
}

check_info() {
    echo -e "${BLUE}ℹ INFO${NC} $1"
}

echo "🏥 TherapyDocs HIPAA Compliance Audit"
echo "===================================="
echo ""

# 1. DATABASE ENCRYPTION CHECKS
echo "📊 DATABASE ENCRYPTION COMPLIANCE"
echo "--------------------------------"

# Check TDE (Transparent Data Encryption)
echo "Checking Transparent Data Encryption (TDE)..."
TDE_STATUS=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
SELECT 
    db.name,
    dek.encryption_state,
    CASE dek.encryption_state 
        WHEN 0 THEN 'No database encryption key present'
        WHEN 1 THEN 'Unencrypted'
        WHEN 2 THEN 'Encryption in progress'
        WHEN 3 THEN 'Encrypted'
        WHEN 4 THEN 'Key change in progress'
        WHEN 5 THEN 'Decryption in progress'
        WHEN 6 THEN 'Protection change in progress'
    END as encryption_status
FROM sys.databases db
LEFT JOIN sys.dm_database_encryption_keys dek ON db.database_id = dek.database_id
WHERE db.name LIKE 'therapydocs_%'
" -h -1 2>/dev/null || echo "ERROR: Could not check TDE status")

if echo "$TDE_STATUS" | grep -q "Encrypted"; then
    check_passed "TDE enabled on TherapyDocs databases"
else
    check_failed "TDE not properly enabled on databases"
fi

# Check Always Encrypted
echo "Checking Always Encrypted configuration..."
AE_KEYS=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) as key_count FROM sys.column_master_keys;
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$AE_KEYS" -gt 0 ]; then
    check_passed "Always Encrypted column master keys configured"
else
    check_failed "Always Encrypted not configured"
fi

# Check encrypted columns
ENCRYPTED_COLUMNS=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM sys.columns c
INNER JOIN sys.column_encryption_keys cek ON c.column_encryption_key_id = cek.column_encryption_key_id;
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$ENCRYPTED_COLUMNS" -gt 0 ]; then
    check_passed "PHI columns encrypted with Always Encrypted ($ENCRYPTED_COLUMNS columns)"
else
    check_failed "No PHI columns found with Always Encrypted"
fi

echo ""

# 2. AUDIT LOGGING CHECKS
echo "📝 AUDIT LOGGING COMPLIANCE"
echo "--------------------------"

# Check SQL Server Audit
AUDIT_STATUS=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
SELECT is_state_enabled FROM sys.server_audits WHERE name = 'TherapyDocs_HIPAA_Audit';
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$AUDIT_STATUS" = "1" ]; then
    check_passed "SQL Server HIPAA audit enabled"
else
    check_failed "SQL Server HIPAA audit not enabled"
fi

# Check audit tables exist
AUDIT_TABLES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('audit');
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$AUDIT_TABLES" -gt 0 ]; then
    check_passed "Audit tables configured ($AUDIT_TABLES tables)"
else
    check_failed "No audit tables found"
fi

# Check temporal tables for audit trail
TEMPORAL_TABLES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM sys.tables WHERE temporal_type = 2;  -- SYSTEM_VERSIONED_TEMPORAL_TABLE
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$TEMPORAL_TABLES" -gt 0 ]; then
    check_passed "Temporal tables for immutable audit trail ($TEMPORAL_TABLES tables)"
else
    check_warning "No temporal tables found for audit trail"
fi

echo ""

# 3. NETWORK SECURITY CHECKS
echo "🔒 NETWORK SECURITY COMPLIANCE"
echo "-----------------------------"

# Check TLS encryption for Redis
echo "Checking Redis TLS encryption..."
if docker-compose exec -T redis-sessions redis-cli -p 6380 --tls --cert /tls/redis.crt --key /tls/redis.key --cacert /tls/ca.crt -a 'R3d1s_S3cur3!' ping 2>/dev/null | grep -q "PONG"; then
    check_passed "Redis Sessions using TLS encryption"
else
    check_failed "Redis Sessions TLS not working"
fi

# Check Elasticsearch HTTPS
if curl -s -k -u elastic:3last1c_H1paa! https://localhost:9200/_cluster/health >/dev/null 2>&1; then
    check_passed "Elasticsearch using HTTPS"
else
    check_failed "Elasticsearch HTTPS not accessible"
fi

# Check if default ports are exposed
EXPOSED_PORTS=$(docker-compose ps --format "table {{.Ports}}" | grep -E "(0\.0\.0\.0|::):" || true)
if [ -z "$EXPOSED_PORTS" ]; then
    check_passed "No services exposed on all interfaces"
else
    check_warning "Some services exposed on all interfaces (review port bindings)"
fi

echo ""

# 4. ACCESS CONTROL CHECKS
echo "👤 ACCESS CONTROL COMPLIANCE"
echo "---------------------------"

# Check user roles table
USER_ROLES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM auth.Roles;
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$USER_ROLES" -ge 7 ]; then
    check_passed "7-role RBAC system configured ($USER_ROLES roles)"
else
    check_failed "RBAC system incomplete (found $USER_ROLES roles, expected 7)"
fi

# Check role names
ROLE_NAMES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT RoleName FROM auth.Roles ORDER BY RoleName;
" -h -1 2>/dev/null || echo "")

expected_roles=("Admin" "Auditor" "Billing" "Parent" "Student" "Supervisor" "Therapist")
for role in "${expected_roles[@]}"; do
    if echo "$ROLE_NAMES" | grep -q "$role"; then
        check_passed "Role '$role' configured"
    else
        check_failed "Role '$role' missing"
    fi
done

echo ""

# 5. DATA RETENTION CHECKS
echo "📅 DATA RETENTION COMPLIANCE"
echo "---------------------------"

# Check audit log retention settings
RETENTION_DAYS=$(grep "AUDIT_LOG_RETENTION_DAYS" ../.env.development | cut -d'=' -f2 || echo "0")
if [ "$RETENTION_DAYS" -ge 2555 ]; then  # 7 years
    check_passed "Audit log retention set to 7+ years ($RETENTION_DAYS days)"
else
    check_failed "Audit log retention insufficient ($RETENTION_DAYS days, required: 2555+)"
fi

echo ""

# 6. BACKUP AND RECOVERY CHECKS
echo "💾 BACKUP AND RECOVERY COMPLIANCE"
echo "--------------------------------"

# Check if backup directories exist
if [ -d "volumes/sqlserver/backup" ]; then
    check_passed "Backup directory structure exists"
else
    check_warning "Backup directory not found (may need manual creation)"
fi

# Check backup encryption capability
BACKUP_CERT=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM sys.certificates WHERE name = 'TDE_Certificate';
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$BACKUP_CERT" -gt 0 ]; then
    check_passed "Backup encryption certificate available"
else
    check_failed "Backup encryption certificate not found"
fi

echo ""

# 7. COMPLIANCE CONFIGURATION CHECKS
echo "⚖️  COMPLIANCE CONFIGURATION"
echo "---------------------------"

# Check state deadline rules
STATE_RULES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(DISTINCT StateCode) FROM compliance.StateDeadlines;
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$STATE_RULES" -ge 4 ]; then
    check_passed "Multi-state compliance rules configured ($STATE_RULES states)"
else
    check_warning "Limited state compliance rules ($STATE_RULES states configured)"
fi

# Check FHIR server
if curl -s http://localhost:8090/fhir/metadata >/dev/null 2>&1; then
    check_passed "FHIR server operational for healthcare interoperability"
else
    check_warning "FHIR server not accessible (may still be starting)"
fi

echo ""

# 8. HEALTHCARE-SPECIFIC CHECKS
echo "🏥 HEALTHCARE-SPECIFIC COMPLIANCE"
echo "-------------------------------"

# Check PHI tables exist
PHI_TABLES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "
USE therapydocs_dev;
SELECT COUNT(*) FROM sys.tables WHERE name IN ('Students', 'SessionNotes', 'PriorAuthorizations');
" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$PHI_TABLES" -eq 3 ]; then
    check_passed "Core PHI tables configured"
else
    check_failed "Core PHI tables missing (found $PHI_TABLES, expected 3)"
fi

# Check healthcare queues
if docker-compose exec -T rabbitmq rabbitmqctl list_queues name 2>/dev/null | grep -q "compliance.deadlines"; then
    check_passed "Healthcare compliance queues configured"
else
    check_warning "Healthcare compliance queues not found"
fi

echo ""

# 9. SECURITY CONFIGURATION CHECKS
echo "🔐 SECURITY CONFIGURATION"
echo "------------------------"

# Check for default passwords (security warning)
DEFAULT_PASSWORDS=$(grep -c "2024!" ../.env.development || echo "0")
if [ "$DEFAULT_PASSWORDS" -gt 0 ]; then
    check_warning "Default passwords detected ($DEFAULT_PASSWORDS instances) - CHANGE BEFORE PRODUCTION"
else
    check_passed "No default passwords detected"
fi

# Check session timeout configuration
SESSION_TIMEOUT=$(grep "SESSION_TIMEOUT_MINUTES" ../.env.development | cut -d'=' -f2 || echo "999")
if [ "$SESSION_TIMEOUT" -le 30 ]; then
    check_passed "HIPAA-compliant session timeout configured ($SESSION_TIMEOUT minutes)"
else
    check_failed "Session timeout too long ($SESSION_TIMEOUT minutes, max: 30)"
fi

echo ""

# SUMMARY
echo "📋 COMPLIANCE AUDIT SUMMARY"
echo "==========================="
echo ""
echo "Test Results:"
echo "  ✓ Passed: $PASSED"
echo "  ✗ Failed: $FAILED"
echo "  ⚠ Warnings: $WARNINGS"
echo ""

TOTAL_TESTS=$((PASSED + FAILED + WARNINGS))
PASS_RATE=$((PASSED * 100 / TOTAL_TESTS))

if [ "$FAILED" -eq 0 ] && [ "$WARNINGS" -le 3 ]; then
    echo -e "${GREEN}🎉 HIPAA COMPLIANCE: EXCELLENT${NC}"
    echo "   Pass Rate: $PASS_RATE%"
    echo "   Status: Ready for development"
elif [ "$FAILED" -le 2 ] && [ "$WARNINGS" -le 5 ]; then
    echo -e "${YELLOW}⚠️  HIPAA COMPLIANCE: GOOD${NC}"
    echo "   Pass Rate: $PASS_RATE%"
    echo "   Status: Address warnings before production"
else
    echo -e "${RED}❌ HIPAA COMPLIANCE: NEEDS ATTENTION${NC}"
    echo "   Pass Rate: $PASS_RATE%"
    echo "   Status: Fix critical issues before proceeding"
fi

echo ""
echo "Next Steps:"
if [ "$FAILED" -gt 0 ]; then
    echo "  1. Fix all failed compliance checks"
fi
if [ "$WARNINGS" -gt 0 ]; then
    echo "  2. Review and address warnings"
fi
echo "  3. Change all default passwords before staging/production"
echo "  4. Implement proper SSL certificates for production"
echo "  5. Configure automated backup procedures"
echo "  6. Set up monitoring and alerting"
echo ""

exit 0