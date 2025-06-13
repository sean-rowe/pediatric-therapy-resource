# TherapyDocs API

## Overview
TherapyDocs API is a .NET 8 Web API that provides backend services for the therapy documentation platform.

## Features Implemented
- Therapist registration with license validation
- Email verification system  
- Password security with bcrypt hashing
- Rate limiting for registration endpoints
- Comprehensive audit logging

## Prerequisites
- .NET 8 SDK
- SQL Server 2022 (or Docker)
- SendGrid account for email services

## Setup

1. **Database Setup**
   ```bash
   # Run migrations
   ../scripts/run-migrations.sh
   ```

2. **Configuration**
   Update `appsettings.Development.json` with your settings:
   - Database connection string
   - SendGrid API key
   - JWT secret key

3. **Run the API**
   ```bash
   dotnet run
   ```

## API Endpoints

### Authentication

#### POST /api/auth/register
Register a new therapist account.

**Request Body:**
```json
{
  "email": "therapist@example.com",
  "password": "SecureP@ssw0rd123!",
  "confirmPassword": "SecureP@ssw0rd123!",
  "firstName": "John",
  "lastName": "Doe",
  "licenseNumber": "ABC12345",
  "licenseState": "CA",
  "licenseType": "OT",
  "phone": "555-123-4567",
  "acceptedTerms": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful! Please check your email to verify your account.",
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

#### GET /api/auth/verify-email/{token}
Verify email address using token from email.

**Response:**
```json
{
  "success": true,
  "message": "Email verified successfully! You can now log in."
}
```

#### POST /api/auth/resend-verification
Resend verification email.

**Request Body:**
```json
{
  "email": "therapist@example.com"
}
```

## Security Features
- Password complexity requirements (12+ chars, uppercase, lowercase, number, special char)
- Common password blacklist
- bcrypt password hashing with work factor 12
- Rate limiting (3 registration attempts per hour per IP)
- SQL injection prevention through parameterized queries
- XSS protection through input validation
- HTTPS enforcement in production

## Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Architecture
- **Controllers**: HTTP endpoint definitions
- **Services**: Business logic implementation
- **Repositories**: Data access layer with stored procedures
- **Models**: Domain entities and DTOs
- **Validators**: FluentValidation rules

## License Verification
The API integrates with state license verification services. Currently supports:
- California (CA)
- New York (NY)
- Texas (TX) 
- Florida (FL)

Additional states can be added by extending the `LicenseVerificationService`.

## Monitoring
- Serilog logging to console and file
- Application Insights integration (production)
- Health checks endpoint at `/health`

## Development
- Swagger UI available at `/swagger` in development
- Hot reload enabled for faster development
- Docker support for containerized deployment