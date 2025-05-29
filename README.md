# TherapyDocs - Mobile Documentation for School Therapists

A mobile-first documentation system for school-based occupational therapists, physical therapists, and speech-language pathologists, featuring AI-powered content generation and digital evaluations.

## Features

- 📱 Mobile-first SOAP note documentation
- 🎯 IEP goal tracking and progress monitoring
- 🤖 AI-powered therapy material generation (mazes, worksheets, activities)
- 📊 Digital evaluation tools with auto-scoring
- 💾 Offline capability with sync
- 📈 Progress reporting and analytics

## Quick Start

### Prerequisites

- Docker and Docker Compose
- Node.js 18+ (for application development)
- Git

### Database Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/therapy-docs.git
cd therapy-docs
```

2. Copy the environment file:
```bash
cp .env.example .env
```

3. Start the MSSQL database:
```bash
./scripts/start-db.sh
```

The database will be automatically initialized with the schema and sample data.

### Database Management

- **Start database**: `./scripts/start-db.sh`
- **Stop database**: `./scripts/stop-db.sh`
- **Reset database**: `./scripts/reset-db.sh` (WARNING: Deletes all data)
- **Connect to database**: `./scripts/connect-db.sh`

### Connection Details

```
Server: localhost,1433
Database: TherapyDocs
Username: SA
Password: TherapyDocs2024!
```

## Database Schema

The system includes 15 core tables:

### Core Documentation Tables
- `users` - Therapist accounts and settings
- `schools` - School locations
- `students` - Student demographics and IEP info
- `iep_goals` - IEP goals and objectives
- `services` - Service assignments
- `appointments` - Session documentation (SOAP notes)

### AI Content Generation Tables
- `content_library` - Generated and pre-made therapy materials
- `content_requests` - AI generation requests
- `content_ratings` - User feedback on content

### Digital Evaluation Tables
- `evaluation_templates` - Assessment tools
- `evaluations` - Assessment instances
- `evaluation_items` - Individual assessment items

### Supporting Tables
- `note_templates` - Documentation templates
- `goal_progress_entries` - Progress tracking
- `audit_log` - HIPAA compliance logging

## Development

### Running SQL Scripts

To run custom SQL scripts:

```bash
docker-compose exec mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "TherapyDocs2024!" -d TherapyDocs -i /path/to/script.sql
```

### Viewing Data

Connect to the database using:
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- VS Code with SQL Server extension
- Command line: `./scripts/connect-db.sh`

## Architecture

- **Database**: Microsoft SQL Server 2022
- **Container**: Docker with official MSSQL image
- **Schema**: Optimized for mobile performance with focused indexes
- **Data Types**: MSSQL-specific (UNIQUEIDENTIFIER, NVARCHAR, etc.)

## Security Considerations

- Change the default SA password in production
- Use environment variables for sensitive data
- Enable TLS/SSL for database connections
- Implement row-level security for multi-tenant scenarios
- Regular backups recommended

## License

[Your License Here]

## Support

For issues or questions, please open an issue on GitHub.