# TherapyDocs System Architecture

## System Overview

TherapyDocs is a multi-tier, cloud-native application designed for scalability, reliability, and security.

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│   Web Client    │     │  iOS/Android    │     │  Parent Portal  │
│  (React SPA)    │     │ (React Native)  │     │   (React PWA)   │
│                 │     │                 │     │                 │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────────────┴───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │                         │
                    │     API Gateway         │
                    │    (Azure API Mgmt)     │
                    │                         │
                    └────────────┬────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌────────▼────────┐     ┌────────▼────────┐     ┌────────▼────────┐
│                 │     │                 │     │                 │
│   Auth API      │     │  Core API       │     │  Billing API    │
│  (.NET 8)       │     │  (.NET 8)       │     │  (.NET 8)       │
│                 │     │                 │     │                 │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────────────┴───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │                         │
                    │    SQL Server 2022      │
                    │   (Primary + Read       │
                    │     Replicas)           │
                    │                         │
                    └────────────┬────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌────────▼────────┐     ┌────────▼────────┐     ┌────────▼────────┐
│                 │     │                 │     │                 │
│  Redis Cache    │     │  Blob Storage   │     │ Service Bus     │
│                 │     │  (Documents)    │     │ (Async Tasks)   │
│                 │     │                 │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

## Technology Stack

### Frontend
- **Web**: React 18 with TypeScript
- **Mobile**: React Native with Expo
- **State Management**: Redux Toolkit with RTK Query
- **UI Framework**: Material-UI v5
- **Testing**: Jest, React Testing Library, Cypress

### Backend
- **Framework**: .NET 8 with C# 12
- **API**: ASP.NET Core Web API
- **ORM**: Entity Framework Core 8 (minimal use)
- **Business Logic**: Stored Procedures (database-first)
- **Authentication**: JWT with refresh tokens
- **Testing**: xUnit, TSQLt

### Database
- **Primary**: SQL Server 2022 Enterprise
- **Caching**: Redis 7
- **Search**: Azure Cognitive Search
- **Storage**: Azure Blob Storage

### Infrastructure
- **Hosting**: Azure Kubernetes Service (AKS)
- **CDN**: Azure Front Door
- **DNS**: Azure DNS
- **Monitoring**: Application Insights
- **CI/CD**: GitHub Actions

### External Services
- **Email**: SendGrid
- **AI/ML**: OpenAI API
- **Analytics**: Azure Monitor
- **Backup**: Azure Backup

## Design Principles

### Database-First Architecture
- Business logic in stored procedures
- Strict input/output contracts
- Comprehensive TSQLt test coverage
- Performance optimization at DB level

### Domain-Driven Design
- Bounded contexts for each subdomain
- Rich domain models
- Aggregate roots for consistency
- Event sourcing for audit trail

### SOLID Principles
- Single Responsibility: One reason to change
- Open/Closed: Extend without modification
- Liskov Substitution: Subtypes substitutable
- Interface Segregation: Specific interfaces
- Dependency Inversion: Depend on abstractions

### Security by Design
- Zero-trust architecture
- Principle of least privilege
- Defense in depth
- Data encryption everywhere

## Deployment Architecture

### Production Environment
- **Region**: East US 2 (Primary), West US 2 (DR)
- **AKS Clusters**: 3 nodes minimum
- **Database**: Always On Availability Groups
- **Storage**: Geo-redundant storage
- **Backup**: 30-day retention

### Staging Environment
- **Region**: East US 2
- **AKS Clusters**: 2 nodes
- **Database**: Single instance
- **Storage**: Locally redundant
- **Backup**: 7-day retention

### Development Environment
- **Local**: Docker Compose
- **Database**: SQL Server Developer
- **Storage**: Local volume mounts
- **Services**: Containerized

## Scalability Strategy

### Horizontal Scaling
- API pods auto-scale 2-20 instances
- Read replicas for reporting
- CDN for static assets
- Queue-based load leveling

### Vertical Scaling
- Database tier elastic pools
- Automatic index tuning
- Query store optimization
- Resource governance

### Data Partitioning
- Partition by school district
- Archive old data yearly
- Separate OLTP/OLAP concerns
- Hot/cold data tiering

## Security Architecture

### Network Security
- VNet isolation
- Network security groups
- DDoS protection
- WAF rules

### Application Security
- OWASP Top 10 compliance
- Input validation
- SQL injection prevention
- XSS protection

### Data Security
- TDE for database
- Column-level encryption
- Key Vault integration
- Backup encryption

### Compliance
- HIPAA controls
- FERPA compliance
- SOC 2 Type II
- PCI DSS (future)