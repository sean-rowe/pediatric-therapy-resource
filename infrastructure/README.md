# TherapyDocs HIPAA-Compliant Infrastructure

This directory contains the complete HIPAA-compliant infrastructure setup for the TherapyDocs therapy documentation platform. The infrastructure is designed with healthcare-grade security, compliance, and operational requirements from day one.

## 🏥 HIPAA Compliance Features

### Data Protection
- **Transparent Data Encryption (TDE)** - All databases encrypted at rest
- **Always Encrypted** - PHI columns encrypted with keys outside database
- **TLS 1.2+** - All network communication encrypted
- **Field-level encryption** - Sensitive data encrypted at application level

### Audit & Compliance
- **Immutable audit logs** - 7-year retention for HIPAA compliance
- **Access logging** - Every PHI access logged with user, time, IP
- **SQL Server Audit** - Database-level audit trails
- **Temporal tables** - System-versioned history tracking

### Access Control
- **7-role RBAC** - Therapist, Supervisor, Billing, Admin, Auditor, Parent, Student
- **Time-bound access** - Substitute teacher temporary permissions
- **Session management** - 30-minute timeout, account lockout
- **Network isolation** - Kubernetes network policies

## 🚀 Quick Start

### Prerequisites
- Docker 20.10+
- Docker Compose 2.0+
- 16GB RAM minimum (recommended: 32GB)
- 100GB available disk space

### Setup Infrastructure
```bash
# Navigate to infrastructure directory
cd infrastructure

# Run the setup script
./scripts/setup-infrastructure.sh

# Verify HIPAA compliance
./scripts/hipaa-audit-check.sh

# Check service health
./health-check.sh
```

### Access Points After Setup
| Service | URL | Default Credentials |
|---------|-----|-------------------|
| SQL Server | localhost:1433 | sa / Th3r@pyD0cs2024! |
| Redis Sessions | localhost:6380 (TLS) | R3d1s_S3cur3! |
| RabbitMQ Management | http://localhost:15672 | therapydocs / R@bb1t_H1paa! |
| Elasticsearch | https://localhost:9200 | elastic / 3last1c_H1paa! |
| Kibana | http://localhost:5601 | elastic / 3last1c_H1paa! |
| HAPI FHIR Server | http://localhost:8090 | No auth (dev) |
| Grafana | http://localhost:3000 | admin / Gr@f@n@_H1paa! |
| Mailhog (Dev) | http://localhost:8025 | No auth |

**⚠️ IMPORTANT: Change all default passwords before staging/production!**

## 📁 Directory Structure

```
infrastructure/
├── docker/
│   ├── docker-compose.yml              # Main development stack
│   ├── docker-compose.prod.yml         # Production overrides
│   └── init-scripts/
│       └── sqlserver/
│           └── 01-init-database.sql     # Database initialization
├── kubernetes/
│   ├── namespaces/                      # Environment namespaces
│   ├── deployments/                     # Service deployments
│   ├── services/                        # Kubernetes services
│   ├── network-policies/                # HIPAA network isolation
│   └── persistent-volumes/              # Storage configuration
├── scripts/
│   ├── setup-infrastructure.sh         # Main setup script
│   ├── hipaa-audit-check.sh            # Compliance verification
│   └── health-check.sh                 # Service health check
└── certs/                              # Development certificates
```

## 🛠️ Services Overview

### Core Data Layer
- **SQL Server 2022 Enterprise** - Primary database with Always Encrypted
- **Redis (Sessions)** - Encrypted session storage with TLS
- **Redis (Cache)** - Application cache (no PHI)

### Healthcare Integration
- **HAPI FHIR R4** - Healthcare interoperability server
- **RabbitMQ** - Healthcare workflow message queues
- **Elasticsearch** - Encrypted search for clinical data

### Monitoring & Compliance
- **Prometheus** - Metrics collection (PHI-free)
- **Grafana** - Compliance and operational dashboards
- **Kibana** - Log analysis and audit trail review

### Development Tools
- **Mailhog** - Email testing
- **Azurite** - Azure Storage emulator

## 🔒 Security Architecture

### Database Security
```sql
-- Always Encrypted for PHI
CREATE COLUMN MASTER KEY [CMK_TherapyDocs] WITH (
    KEY_STORE_PROVIDER_NAME = N'MSSQL_CERTIFICATE_STORE',
    KEY_PATH = N'CurrentUser/My/TherapyDocsCMK'
);

-- Encrypted columns for PHI data
FirstName NVARCHAR(100) ENCRYPTED WITH (
    COLUMN_ENCRYPTION_KEY = [CEK_PHI], 
    ENCRYPTION_TYPE = Deterministic
)

-- Temporal tables for audit trail
WITH (SYSTEM_VERSIONING = ON)
```

### Network Security
- **Network Policies** - Kubernetes-based isolation
- **TLS Everywhere** - No unencrypted communication
- **Port Restrictions** - Minimal external exposure
- **Service Mesh Ready** - Prepared for Istio/Linkerd

### Application Security
- **JWT Authentication** - Stateless with refresh tokens
- **Role-Based Access** - 7-role healthcare hierarchy
- **Session Management** - HIPAA-compliant timeouts
- **Audit Logging** - Every action logged

## 🏥 Healthcare-Specific Features

### State Compliance Engine
```sql
-- State-specific documentation deadlines
INSERT INTO compliance.StateDeadlines VALUES
('TX', 'SLP', 'Progress Report', 5, 1, 2, 1),  -- Texas: 5 business days
('CA', 'SLP', 'Progress Report', 3, 1, 1, 1),  -- California: 3 business days
('NY', 'SLP', 'Progress Report', 7, 0, 2, 0);  -- New York: 7 calendar days
```

### Message Queues for Healthcare Workflows
- `auth.alerts` - Security and login alerts
- `compliance.deadlines` - Documentation deadline warnings
- `billing.prior_auth_warnings` - Authorization expiration alerts
- `notifications.email` - HIPAA-compliant notifications

### PHI Data Protection
- **Students Table** - Encrypted names, DOB, medical alerts
- **Session Notes** - Encrypted SOAP documentation
- **Prior Authorizations** - Encrypted billing information
- **Audit Trail** - Immutable access logging

## 📊 Monitoring & Compliance

### HIPAA Audit Dashboard
- PHI access patterns
- Failed login attempts
- Data export activities
- Compliance deadline status

### Performance Monitoring
- Database query performance
- Session response times
- Encryption overhead
- Queue processing rates

### Health Checks
```bash
# Comprehensive health check
./health-check.sh

# HIPAA compliance audit
./scripts/hipaa-audit-check.sh

# Individual service status
docker-compose ps
```

## 🚢 Deployment Environments

### Development (Current)
- Docker Compose
- Self-signed certificates
- Development data
- Full feature access

### Staging (Future)
- Kubernetes on Azure AKS
- Let's Encrypt certificates
- Synthetic test data
- Production-like security

### Production (Future)
- Azure AKS with multiple availability zones
- Azure Key Vault for secrets
- Real SSL certificates
- Full HIPAA compliance audit

## 🔧 Configuration Management

### Environment Variables
- `.env.development` - Development configuration
- `.env.staging` - Staging overrides
- `.env.production` - Production secrets (not in repo)

### Key Configuration Areas
```bash
# Database encryption
MSSQL_CONNECTION="...;Column Encryption Setting=Enabled"

# Session security
SESSION_TIMEOUT_MINUTES=30
MOBILE_SESSION_TIMEOUT_MINUTES=15

# Audit retention (7 years for HIPAA)
AUDIT_LOG_RETENTION_DAYS=2555

# Feature flags
ENABLE_VOICE_TO_TEXT=true
ENABLE_PRIOR_AUTH_TRACKING=true
```

## 🧪 Testing & Validation

### Compliance Testing
```bash
# Run full HIPAA audit
./scripts/hipaa-audit-check.sh

# Test encryption
./scripts/test-encryption.sh

# Validate audit logs
./scripts/validate-audit-trail.sh
```

### Performance Testing
```bash
# Database performance
./scripts/test-db-performance.sh

# Encryption overhead
./scripts/test-encryption-performance.sh

# Load testing
./scripts/load-test.sh
```

## 📋 Production Readiness Checklist

### Before Staging Deployment
- [ ] Change all default passwords
- [ ] Configure proper SSL certificates
- [ ] Set up Azure Key Vault
- [ ] Configure backup automation
- [ ] Test disaster recovery
- [ ] Security penetration testing

### Before Production Deployment
- [ ] HIPAA compliance audit by third party
- [ ] Insurance approval for cybersecurity
- [ ] Staff training on security procedures
- [ ] Incident response plan
- [ ] 24/7 monitoring setup
- [ ] Business Associate Agreements (BAAs)

## 🆘 Troubleshooting

### Common Issues

**SQL Server won't start**
```bash
# Check logs
docker-compose logs sqlserver

# Reset data
rm -rf volumes/sqlserver/data/*
docker-compose up sqlserver
```

**Elasticsearch security errors**
```bash
# Reset passwords
docker-compose exec elasticsearch elasticsearch-setup-passwords interactive
```

**Redis TLS connection issues**
```bash
# Regenerate certificates
./scripts/generate-certs.sh
docker-compose restart redis-sessions
```

### Support Contacts
- Infrastructure Issues: DevOps Team
- Security Questions: Security Team
- HIPAA Compliance: Compliance Officer

## 📚 Additional Resources

- [HIPAA Security Rule](https://www.hhs.gov/hipaa/for-professionals/security/index.html)
- [Azure SQL Always Encrypted](https://docs.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine)
- [Kubernetes Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [HAPI FHIR Documentation](https://hapifhir.io/hapi-fhir/docs/)

---

**🔒 This infrastructure is designed for HIPAA compliance but requires proper operational procedures and staff training to maintain compliance in production.**