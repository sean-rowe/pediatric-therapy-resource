# Tech: Student Management - API Endpoints

## Story
As a frontend developer  
I want to have comprehensive REST API endpoints for student management  
So that I can build a responsive and feature-rich student management interface

## Acceptance Criteria

### Core Student Endpoints

#### GET /api/students
- [ ] Returns paginated list of students
- [ ] Query parameters:
  - `page` (default: 1)
  - `pageSize` (default: 20, max: 100)
  - `search` - searches name and student ID
  - `schoolId` - filter by school
  - `gradeLevel` - filter by grade
  - `hasIep` - filter by IEP status (true/false)
  - `isActive` - filter by active status (default: true)
  - `sortBy` - field to sort by (default: lastName)
  - `sortOrder` - asc/desc (default: asc)
- [ ] Response includes:
  ```json
  {
    "data": [{
      "id": "guid",
      "firstName": "string",
      "lastName": "string",
      "studentId": "string",
      "gradeLevel": "string",
      "schoolName": "string",
      "hasIep": boolean,
      "isActive": boolean
    }],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 150,
      "totalPages": 8
    }
  }
  ```
- [ ] Returns 200 OK with results
- [ ] Implements proper authorization checks

#### GET /api/students/{id}
- [ ] Returns complete student details
- [ ] Includes all demographic information
- [ ] Includes IEP details if exists
- [ ] Includes parent contacts
- [ ] Includes medical alerts and behavioral notes
- [ ] Response structure:
  ```json
  {
    "id": "guid",
    "firstName": "string",
    "lastName": "string",
    "dateOfBirth": "date",
    "gradeLevel": "string",
    "school": {
      "id": "guid",
      "name": "string",
      "district": "string"
    },
    "studentId": "string",
    "caseManager": "string",
    "iep": {
      "hasIep": boolean,
      "startDate": "date",
      "endDate": "date",
      "primaryDisability": "string"
    },
    "medicalAlerts": "string",
    "behavioralNotes": "string",
    "parents": [{
      "id": "guid",
      "name": "string",
      "phone": "string",
      "email": "string",
      "relationship": "string"
    }],
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
  ```
- [ ] Returns 404 if student not found
- [ ] Returns 403 if not authorized to view

#### POST /api/students
- [ ] Creates new student record
- [ ] Request body validation:
  - Required: firstName, lastName, dateOfBirth, schoolId
  - Optional: all other fields
- [ ] Validates student ID uniqueness within district
- [ ] Creates audit log entry
- [ ] Returns 201 Created with new student data
- [ ] Returns 400 for validation errors
- [ ] Returns 409 if student ID already exists

#### PUT /api/students/{id}
- [ ] Updates existing student
- [ ] Supports partial updates (PATCH-like behavior)
- [ ] Validates all provided fields
- [ ] Updates timestamp
- [ ] Creates audit log entry
- [ ] Returns 200 OK with updated data
- [ ] Returns 404 if student not found
- [ ] Returns 400 for validation errors

#### DELETE /api/students/{id}
- [ ] Soft deletes (deactivates) student
- [ ] Request body: `{ "reason": "string" }`
- [ ] Sets isActive to false
- [ ] Creates audit log entry
- [ ] Returns 204 No Content
- [ ] Returns 404 if student not found

### Specialized Endpoints

#### POST /api/students/{id}/reactivate
- [ ] Reactivates deactivated student
- [ ] Returns 200 OK with updated student
- [ ] Returns 404 if student not found
- [ ] Returns 400 if student already active

#### POST /api/students/{id}/transfer
- [ ] Transfers student to different school
- [ ] Request body:
  ```json
  {
    "toSchoolId": "guid",
    "transferDate": "date",
    "reason": "string"
  }
  ```
- [ ] Updates student's school
- [ ] Creates transfer history record
- [ ] Returns 200 OK with updated student
- [ ] Returns 400 for validation errors

#### GET /api/students/{id}/history
- [ ] Returns complete student history
- [ ] Includes:
  - Service history
  - Therapist assignments
  - Goals and progress
  - Evaluations
  - School transfers
- [ ] Returns paginated results
- [ ] Returns 404 if student not found

### IEP Management Endpoints

#### PUT /api/students/{id}/iep
- [ ] Adds or updates IEP information
- [ ] Request body:
  ```json
  {
    "hasIep": boolean,
    "startDate": "date",
    "endDate": "date",
    "primaryDisability": "string"
  }
  ```
- [ ] Validates date ranges
- [ ] Creates audit log entry
- [ ] Returns 200 OK with updated student

#### DELETE /api/students/{id}/iep
- [ ] Removes IEP information
- [ ] Sets hasIep to false
- [ ] Maintains IEP history
- [ ] Returns 204 No Content

### Parent Management Endpoints

#### GET /api/students/{id}/parents
- [ ] Returns all parent contacts for student
- [ ] Includes relationship and permissions
- [ ] Returns 200 OK with parent list

#### POST /api/students/{id}/parents
- [ ] Adds new parent contact
- [ ] Request body includes parent details and relationship
- [ ] Creates parent account if needed
- [ ] Returns 201 Created with new parent data

#### PUT /api/students/{id}/parents/{parentId}
- [ ] Updates parent contact information
- [ ] Returns 200 OK with updated data
- [ ] Returns 404 if parent or student not found

#### DELETE /api/students/{id}/parents/{parentId}
- [ ] Removes parent access
- [ ] Soft delete maintaining history
- [ ] Returns 204 No Content

### Batch Operations

#### POST /api/students/batch
- [ ] Creates multiple students at once
- [ ] Request body: array of student objects
- [ ] Validates all records before processing
- [ ] Uses transaction for atomicity
- [ ] Returns 207 Multi-Status with results:
  ```json
  {
    "successful": 45,
    "failed": 5,
    "errors": [{
      "index": 2,
      "studentId": "SW123",
      "error": "Student ID already exists"
    }]
  }
  ```

#### PUT /api/students/batch
- [ ] Updates multiple students at once
- [ ] Request body: array of updates with student IDs
- [ ] Returns 207 Multi-Status with results

### Search and Analytics

#### GET /api/students/search
- [ ] Advanced search endpoint
- [ ] Supports complex queries
- [ ] Full-text search capabilities
- [ ] Returns highlighted results

#### GET /api/students/statistics
- [ ] Returns aggregate statistics
- [ ] Query parameters for filtering by school/district
- [ ] Includes:
  - Total students
  - Active IEPs
  - Grade distribution
  - Disability categories
  - Risk indicators

#### GET /api/students/at-risk
- [ ] Returns students with risk indicators
- [ ] Filters by risk type and severity
- [ ] Includes recommended actions
- [ ] Used for early intervention

### Export Endpoints

#### GET /api/students/export
- [ ] Exports student data
- [ ] Query parameters for filtering
- [ ] Format parameter: csv, excel, pdf
- [ ] Returns file download
- [ ] Respects data privacy settings

### Error Responses
- All endpoints return consistent error format:
  ```json
  {
    "error": {
      "code": "STUDENT_NOT_FOUND",
      "message": "Student with ID xyz not found",
      "details": {}
    }
  }
  ```

### Security Requirements
- [ ] All endpoints require authentication
- [ ] Role-based authorization checks
- [ ] Therapists can only access assigned students
- [ ] Administrators have full access
- [ ] API rate limiting implemented
- [ ] Request/response logging for audit

### Performance Requirements
- [ ] Response time < 200ms for single records
- [ ] Response time < 500ms for lists
- [ ] Implements caching where appropriate
- [ ] Database connection pooling
- [ ] Pagination for all list endpoints

## Story Points
13

## Dependencies
- Database procedures implemented
- Authentication/authorization middleware
- API framework setup (e.g., ASP.NET Core)
- Validation framework
- Logging infrastructure

## Technical Notes
- RESTful design principles
- Consistent URL patterns
- Proper HTTP status codes
- Content negotiation support
- CORS configuration for frontend
- OpenAPI/Swagger documentation
- Integration tests for all endpoints