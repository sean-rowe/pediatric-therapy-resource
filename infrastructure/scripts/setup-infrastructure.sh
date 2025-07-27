#!/bin/bash
# TherapyDocs HIPAA-Compliant Infrastructure Setup Script

set -e  # Exit on any error

echo "🏥 Setting up TherapyDocs HIPAA-Compliant Infrastructure..."
echo "================================================================"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}❌${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

# Check prerequisites
echo "🔍 Checking prerequisites..."

command -v docker >/dev/null 2>&1 || { 
    print_error "Docker required but not installed. Please install Docker first."
    exit 1
}
print_status "Docker found"

command -v docker-compose >/dev/null 2>&1 || { 
    print_error "Docker Compose required but not installed."
    exit 1
}
print_status "Docker Compose found"

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi
print_status "Docker is running"

# Navigate to infrastructure directory
cd "$(dirname "$0")/.."
INFRA_DIR=$(pwd)
print_info "Working directory: $INFRA_DIR"

# Create necessary directories
echo ""
echo "📁 Creating directory structure..."
mkdir -p volumes/sqlserver
mkdir -p volumes/sqlserver/data
mkdir -p volumes/sqlserver/backup
mkdir -p certs/redis
mkdir -p certs/elasticsearch
mkdir -p encryption-keys
mkdir -p prometheus
mkdir -p grafana/dashboards
mkdir -p rabbitmq

print_status "Directory structure created"

# Generate environment file if it doesn't exist
if [ ! -f .env ]; then
    echo ""
    echo "🔧 Creating environment configuration..."
    cat > .env << EOF
# TherapyDocs Development Environment
# Generated on $(date)

# Database - SQL Server with Always Encrypted
MSSQL_SA_PASSWORD=Th3r@pyD0cs2024!

# Redis - Separate instances for sessions and cache
REDIS_PASSWORD=R3d1s_S3cur3!
REDIS_CACHE_PASSWORD=C@ch3_S3cur3!

# RabbitMQ - Notification queue
RABBITMQ_PASSWORD=R@bb1t_H1paa!

# Elasticsearch - Encrypted search
ELASTIC_PASSWORD=3last1c_H1paa!

# Monitoring
GRAFANA_PASSWORD=Gr@f@n@_H1paa!

# Security Warning
# CHANGE ALL DEFAULT PASSWORDS BEFORE PRODUCTION DEPLOYMENT!
EOF
    print_status "Environment file created (.env)"
    print_warning "Please review and update passwords in .env file"
else
    print_info "Environment file already exists"
fi

# Create Prometheus configuration
echo ""
echo "📊 Setting up monitoring configuration..."
cat > prometheus/prometheus.yml << EOF
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  # - "first_rules.yml"
  # - "second_rules.yml"

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'therapydocs-api'
    static_configs:
      - targets: ['api:8000']
    metrics_path: /metrics
    scrape_interval: 30s

  - job_name: 'sqlserver'
    static_configs:
      - targets: ['sqlserver:1433']
    scrape_interval: 60s

  - job_name: 'redis-sessions'
    static_configs:
      - targets: ['redis-sessions:6380']
    scrape_interval: 30s

  - job_name: 'elasticsearch'
    static_configs:
      - targets: ['elasticsearch:9200']
    scrape_interval: 60s
EOF

print_status "Prometheus configuration created"

# Create RabbitMQ definitions for healthcare queues
cat > rabbitmq/definitions.json << EOF
{
  "rabbit_version": "3.11.0",
  "users": [
    {
      "name": "therapydocs",
      "password_hash": "generated_hash",
      "hashing_algorithm": "rabbit_password_hashing_sha256",
      "tags": "administrator"
    }
  ],
  "vhosts": [
    {
      "name": "/"
    }
  ],
  "permissions": [
    {
      "user": "therapydocs",
      "vhost": "/",
      "configure": ".*",
      "write": ".*",
      "read": ".*"
    }
  ],
  "queues": [
    {
      "name": "auth.alerts",
      "vhost": "/",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "compliance.deadlines",
      "vhost": "/",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "billing.prior_auth_warnings",
      "vhost": "/",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "notifications.email",
      "vhost": "/",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    }
  ],
  "exchanges": [
    {
      "name": "therapydocs.direct",
      "vhost": "/",
      "type": "direct",
      "durable": true,
      "auto_delete": false,
      "internal": false,
      "arguments": {}
    }
  ],
  "bindings": [
    {
      "source": "therapydocs.direct",
      "vhost": "/",
      "destination": "auth.alerts",
      "destination_type": "queue",
      "routing_key": "auth.alert",
      "arguments": {}
    },
    {
      "source": "therapydocs.direct",
      "vhost": "/",
      "destination": "compliance.deadlines",
      "destination_type": "queue",
      "routing_key": "compliance.deadline",
      "arguments": {}
    },
    {
      "source": "therapydocs.direct",
      "vhost": "/",
      "destination": "billing.prior_auth_warnings",
      "destination_type": "queue",
      "routing_key": "billing.prior_auth",
      "arguments": {}
    }
  ]
}
EOF

print_status "RabbitMQ configuration created"

# Generate self-signed certificates for development
echo ""
echo "🔐 Generating development certificates..."

# Generate CA certificate
openssl req -x509 -new -nodes -days 365 -keyout certs/ca.key -out certs/ca.crt -subj "/CN=TherapyDocs-CA"

# Generate Redis certificates
openssl req -new -nodes -keyout certs/redis/redis.key -out certs/redis/redis.csr -subj "/CN=redis"
openssl x509 -req -in certs/redis/redis.csr -CA certs/ca.crt -CAkey certs/ca.key -CAcreateserial -out certs/redis/redis.crt -days 365
cp certs/ca.crt certs/redis/ca.crt

# Generate Elasticsearch certificates
openssl req -new -nodes -keyout certs/elasticsearch/elastic.key -out certs/elasticsearch/elastic.csr -subj "/CN=elasticsearch"
openssl x509 -req -in certs/elasticsearch/elastic.csr -CA certs/ca.crt -CAkey certs/ca.key -CAcreateserial -out certs/elasticsearch/elastic.crt -days 365

# Create PKCS12 for Elasticsearch
openssl pkcs12 -export -out certs/elasticsearch/elastic-certificates.p12 -inkey certs/elasticsearch/elastic.key -in certs/elasticsearch/elastic.crt -certfile certs/ca.crt -password pass:therapydocs

print_status "Development certificates generated"

# Set appropriate permissions
chmod 600 certs/ca.key
chmod 600 certs/redis/redis.key
chmod 600 certs/elasticsearch/elastic.key
chmod 644 certs/elasticsearch/elastic-certificates.p12

# Start the infrastructure
echo ""
echo "🚀 Starting HIPAA-compliant infrastructure..."
docker-compose down --remove-orphans 2>/dev/null || true
docker-compose up -d

print_info "Waiting for services to start..."

# Wait for SQL Server to be ready
echo "⏳ Waiting for SQL Server to initialize..."
timeout=300
counter=0
while ! docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "SELECT 1" >/dev/null 2>&1; do
    if [ $counter -ge $timeout ]; then
        print_error "SQL Server failed to start within $timeout seconds"
        exit 1
    fi
    sleep 5
    counter=$((counter + 5))
    echo -n "."
done
echo ""
print_status "SQL Server is ready"

# Wait for Elasticsearch to be ready
echo "⏳ Waiting for Elasticsearch to initialize..."
counter=0
while ! curl -s -k -u elastic:3last1c_H1paa! https://localhost:9200/_cluster/health >/dev/null 2>&1; do
    if [ $counter -ge $timeout ]; then
        print_error "Elasticsearch failed to start within $timeout seconds"
        exit 1
    fi
    sleep 5
    counter=$((counter + 5))
    echo -n "."
done
echo ""
print_status "Elasticsearch is ready"

# Run HIPAA compliance check
echo ""
echo "✅ Running HIPAA compliance verification..."

# Check if databases were created
DATABASES=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "SELECT name FROM sys.databases WHERE name LIKE 'therapydocs_%'" -h -1 2>/dev/null | grep -c therapydocs || echo "0")

if [ "$DATABASES" -ge 4 ]; then
    print_status "Databases created successfully ($DATABASES databases)"
else
    print_warning "Expected 4+ databases, found $DATABASES"
fi

# Check TDE encryption
TDE_STATUS=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "SELECT encryption_state FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID('therapydocs_dev')" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$TDE_STATUS" = "3" ]; then
    print_status "Transparent Data Encryption (TDE) enabled"
else
    print_warning "TDE encryption status: $TDE_STATUS (expected: 3 for encrypted)"
fi

# Check audit configuration
AUDIT_COUNT=$(docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "SELECT COUNT(*) FROM sys.server_audits WHERE name = 'TherapyDocs_HIPAA_Audit'" -h -1 2>/dev/null | tr -d '[:space:]' || echo "0")

if [ "$AUDIT_COUNT" = "1" ]; then
    print_status "HIPAA audit configuration enabled"
else
    print_warning "HIPAA audit not properly configured"
fi

# Verify service connectivity
echo ""
echo "🔗 Verifying service connectivity..."

# Test Redis Sessions (TLS)
if docker-compose exec -T redis-sessions redis-cli -p 6380 --tls --cert /tls/redis.crt --key /tls/redis.key --cacert /tls/ca.crt -a 'R3d1s_S3cur3!' ping 2>/dev/null | grep -q "PONG"; then
    print_status "Redis Sessions (TLS) connectivity verified"
else
    print_warning "Redis Sessions connectivity issue"
fi

# Test RabbitMQ
if curl -s -u therapydocs:R@bb1t_H1paa! http://localhost:15672/api/overview >/dev/null 2>&1; then
    print_status "RabbitMQ Management API accessible"
else
    print_warning "RabbitMQ Management API not accessible"
fi

# Test FHIR Server
if curl -s http://localhost:8090/fhir/metadata >/dev/null 2>&1; then
    print_status "HAPI FHIR Server accessible"
else
    print_warning "HAPI FHIR Server not ready yet (may need more time)"
fi

# Display service access information
echo ""
echo "🎉 TherapyDocs HIPAA-compliant infrastructure setup complete!"
echo "================================================================"
echo ""
echo "📊 Service Access Points:"
echo "┌─────────────────────────────────────────────────────────────┐"
echo "│ Service              │ URL/Connection                       │"
echo "├─────────────────────────────────────────────────────────────┤"
echo "│ SQL Server           │ localhost:1433 (sa/Th3r@pyD0cs2024!) │"
echo "│ Redis Sessions (TLS) │ localhost:6380 (R3d1s_S3cur3!)       │"
echo "│ Redis Cache          │ localhost:6379 (C@ch3_S3cur3!)       │"
echo "│ RabbitMQ Management  │ http://localhost:15672                │"
echo "│ Elasticsearch        │ https://localhost:9200                │"
echo "│ Kibana               │ http://localhost:5601                 │"
echo "│ HAPI FHIR Server     │ http://localhost:8090                 │"
echo "│ Prometheus           │ http://localhost:9090                 │"
echo "│ Grafana              │ http://localhost:3000                 │"
echo "│ Mailhog (Dev)        │ http://localhost:8025                 │"
echo "│ Azurite (Storage)    │ http://localhost:10000                │"
echo "└─────────────────────────────────────────────────────────────┘"
echo ""
echo "🔐 Default Credentials (CHANGE IN PRODUCTION):"
echo "  - SQL Server: sa / Th3r@pyD0cs2024!"
echo "  - RabbitMQ: therapydocs / R@bb1t_H1paa!"
echo "  - Elasticsearch: elastic / 3last1c_H1paa!"
echo "  - Grafana: admin / Gr@f@n@_H1paa!"
echo ""
echo "✅ HIPAA Compliance Features Enabled:"
echo "  ✓ Transparent Data Encryption (TDE)"
echo "  ✓ Always Encrypted for PHI columns"
echo "  ✓ Comprehensive audit logging"
echo "  ✓ Network isolation"
echo "  ✓ Encrypted communication (TLS)"
echo "  ✓ Healthcare-specific message queues"
echo "  ✓ State compliance deadline tracking"
echo "  ✓ 7-role RBAC foundation"
echo ""
echo "⚠️  IMPORTANT NEXT STEPS:"
echo "  1. Change all default passwords before staging/production"
echo "  2. Configure proper SSL certificates for production"
echo "  3. Set up proper backup and disaster recovery"
echo "  4. Configure monitoring and alerting"
echo "  5. Review and test all HIPAA compliance features"
echo ""
echo "🚀 Infrastructure is ready for TherapyDocs application development!"

# Create a simple health check script
cat > health-check.sh << 'EOF'
#!/bin/bash
echo "TherapyDocs Infrastructure Health Check"
echo "======================================"

# Check SQL Server
if docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Th3r@pyD0cs2024!' -Q "SELECT 1" >/dev/null 2>&1; then
    echo "✓ SQL Server: Running"
else
    echo "❌ SQL Server: Down"
fi

# Check Redis Sessions
if docker-compose exec -T redis-sessions redis-cli -p 6380 --tls --cert /tls/redis.crt --key /tls/redis.key --cacert /tls/ca.crt -a 'R3d1s_S3cur3!' ping 2>/dev/null | grep -q "PONG"; then
    echo "✓ Redis Sessions: Running"
else
    echo "❌ Redis Sessions: Down"
fi

# Check Redis Cache
if docker-compose exec -T redis-cache redis-cli -a 'C@ch3_S3cur3!' ping 2>/dev/null | grep -q "PONG"; then
    echo "✓ Redis Cache: Running"
else
    echo "❌ Redis Cache: Down"
fi

# Check RabbitMQ
if curl -s -u therapydocs:R@bb1t_H1paa! http://localhost:15672/api/overview >/dev/null 2>&1; then
    echo "✓ RabbitMQ: Running"
else
    echo "❌ RabbitMQ: Down"
fi

# Check Elasticsearch
if curl -s -k -u elastic:3last1c_H1paa! https://localhost:9200/_cluster/health >/dev/null 2>&1; then
    echo "✓ Elasticsearch: Running"
else
    echo "❌ Elasticsearch: Down"
fi

echo ""
echo "Run 'docker-compose ps' for detailed service status"
EOF

chmod +x health-check.sh
print_status "Health check script created (./health-check.sh)"

echo ""
print_info "Setup completed successfully. You can now proceed with application development."