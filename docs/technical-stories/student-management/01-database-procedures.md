# Tech: Student Management - Database Procedures

## Story
As a backend developer  
I want to implement all database procedures for student management  
So that we have a robust and performant data access layer for student operations

## Acceptance Criteria

### CRUD Procedures
- [ ] `sp_student_create` - Creates new student with all demographic information
  - Validates required fields (first_name, last_name, date_of_birth, school_id)
  - Generates unique system ID (GUID)
  - Sets created_at and updated_at timestamps
  - Returns newly created student record
  - Creates audit log entry for 'student_created'
  
- [ ] `sp_student_read` - Retrieves student by ID
  - Accepts student_id parameter
  - Returns complete student record including related data
  - Includes IEP information if exists
  - Includes medical alerts and behavioral notes
  - Includes parent contact information
  
- [ ] `sp_student_update` - Updates existing student information
  - Accepts student_id and fields to update
  - Validates data types and constraints
  - Updates updated_at timestamp
  - Creates audit log entry for 'student_updated'
  - Returns updated student record
  
- [ ] `sp_student_delete` - Soft deletes student (deactivation)
  - Accepts student_id and deactivation_reason
  - Sets is_active = 0
  - Sets deactivated_at timestamp
  - Preserves all student data
  - Creates audit log entry for 'student_deactivated'

### Search and Filter Procedures
- [ ] `sp_student_search` - Advanced search functionality
  - Parameters: search_text, school_id, grade_level, has_iep, is_active
  - Searches across first_name, last_name, student_id
  - Supports partial matching
  - Returns paginated results (offset/limit)
  - Orders by last_name, first_name by default
  
- [ ] `sp_student_get_by_therapist` - Gets students assigned to therapist
  - Accepts therapist_id parameter
  - Returns only students with active assignments
  - Includes assignment details (start_date, service_type)
  - Orders by last_name, first_name
  
- [ ] `sp_student_get_by_school` - Gets all students in a school
  - Accepts school_id parameter
  - Optional filter for active_only
  - Returns student list with basic info
  - Includes grade level and IEP status

### Specialized Procedures
- [ ] `sp_student_transfer` - Transfers student between schools
  - Parameters: student_id, from_school_id, to_school_id, transfer_date
  - Updates student's school_id
  - Creates transfer history record
  - Updates any active sessions/schedules
  - Creates audit log entry for 'student_transferred'
  
- [ ] `sp_student_archive` - Archives inactive students
  - Identifies students inactive > 2 years
  - Moves data to archive tables
  - Maintains referential integrity
  - Creates archive summary report
  
- [ ] `sp_student_validate_unique_id` - Validates student ID uniqueness
  - Parameters: student_id, district_id
  - Checks if student_id exists in district
  - Returns boolean result
  - Used before creating new students
  
- [ ] `sp_student_bulk_import` - Imports multiple students
  - Accepts table-valued parameter with student data
  - Validates all records before importing
  - Uses transaction for all-or-nothing import
  - Returns import summary (success/failure counts)
  - Creates audit log entries for each import

### IEP Management Procedures
- [ ] `sp_student_iep_add` - Adds IEP information to student
  - Parameters: student_id, start_date, end_date, primary_disability
  - Sets has_iep = 1
  - Validates date ranges
  - Creates audit log entry for 'iep_added'
  
- [ ] `sp_student_iep_update` - Updates existing IEP information
  - Updates IEP dates and disability information
  - Maintains IEP history
  - Creates audit log entry for 'iep_updated'
  
- [ ] `sp_student_iep_expire` - Handles IEP expiration
  - Identifies students with expired IEPs
  - Updates has_iep status
  - Sends notifications for upcoming expirations

### Parent Management Procedures
- [ ] `sp_student_parent_add` - Adds parent contact to student
  - Links parent account to student
  - Sets relationship type and permissions
  - Validates unique parent-student relationship
  
- [ ] `sp_student_parent_update` - Updates parent contact information
  - Updates parent details
  - Maintains contact history
  - Creates audit log entry
  
- [ ] `sp_student_parent_remove` - Removes parent access
  - Soft deletes parent-student relationship
  - Maintains historical record
  - Updates parent portal access

### Reporting Procedures
- [ ] `sp_student_statistics_by_school` - Gets student statistics
  - Total students, active IEPs, grade distribution
  - Disability category breakdown
  - Service utilization rates
  
- [ ] `sp_student_risk_indicators_calculate` - Calculates risk indicators
  - Analyzes attendance, progress, engagement
  - Updates student_risk_indicators table
  - Identifies at-risk students
  
- [ ] `sp_student_history_get` - Retrieves complete student history
  - All services received
  - All therapists assigned
  - All goals and progress
  - All evaluations and assessments

### Error Handling
- All procedures include TRY-CATCH blocks
- Meaningful error messages returned
- Transaction rollback on errors
- Error logging to system log table

### Performance Considerations
- Appropriate indexes on search columns
- Query optimization for large datasets
- Use of table hints where appropriate
- Execution plan analysis for complex queries

## Story Points
8

## Dependencies
- Database schema must be finalized
- Audit logging infrastructure in place
- Error handling standards defined

## Technical Notes
- Follow naming convention: sp_[entity]_[action]
- Use consistent parameter naming
- Include header comments with purpose and parameters
- Add unit tests using tSQLt framework
- Document any complex business logic