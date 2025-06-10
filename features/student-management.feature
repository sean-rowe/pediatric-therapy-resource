Feature: Student Management
  As a therapist
  I want to manage student information
  So that I can provide appropriate therapy services

  Background:
    Given I am logged in as a therapist
    And the following schools exist:
      | ID | Name                | District         |
      | 1  | Lincoln Elementary  | Springfield ISD  |
      | 2  | Washington Middle   | Springfield ISD  |

  Rule: Students must have complete demographic information

    Scenario: Create student with complete information
      Given I have permission to add students
      When I create a student with:
        | Field              | Value                      |
        | First Name         | Emma                       |
        | Last Name          | Wilson                     |
        | Date of Birth      | 2015-03-15                |
        | Grade Level        | 3rd                       |
        | School ID          | 1                         |
        | Student ID         | SW12345                   |
        | Has IEP           | true                      |
        | Primary Disability | Developmental Delay       |
        | Medical Alerts     | Uses hearing aids         |
        | Parent Name        | Lisa Wilson               |
        | Parent Phone       | 555-0123                  |
        | Parent Email       | lisa.wilson@email.com     |
      Then the student is created successfully
      And the student has a unique system ID
      And the student is marked as active
      And an audit log entry is created for "student_created"

    Scenario: Student ID must be unique within district
      Given a student exists with ID "SW12345" in district "Springfield ISD"
      When I attempt to create a student with ID "SW12345" in school "1"
      Then the creation fails with error "Student ID already exists in district"

    Scenario Outline: Required fields validation
      When I attempt to create a student without "<field>"
      Then the creation fails with error "<error>"

      Examples:
        | field         | error                       |
        | First Name    | First name is required      |
        | Last Name     | Last name is required       |
        | Date of Birth | Date of birth is required   |
        | School ID     | School is required          |

  Rule: Student information can be updated

    Scenario: Update student demographics
      Given a student "Emma Wilson" exists
      When I update the student with:
        | Field       | Value |
        | Grade Level | 4th   |
      Then the update is successful
      And the student's grade level is "4th"
      And the updated_at timestamp is current
      And an audit log entry is created for "student_updated"

    Scenario: Update IEP information
      Given a student "Emma Wilson" exists with no IEP
      When I update the student with:
        | Field              | Value                   |
        | Has IEP           | true                   |
        | IEP Start Date    | 2024-01-15            |
        | IEP End Date      | 2025-01-14            |
        | Primary Disability | Autism Spectrum Disorder |
      Then the update is successful
      And the student has an active IEP
      And an audit log entry is created for "iep_added"

    Scenario: Add medical alert
      Given a student "Emma Wilson" exists
      When I add medical alert "Seizure disorder - requires medication at noon"
      Then the medical alert is added
      And therapists see the alert when viewing the student

  Rule: Students can be searched and filtered

    Scenario: Search student by name
      Given the following students exist:
        | First Name | Last Name | School ID |
        | Emma       | Wilson    | 1         |
        | Emma       | Johnson   | 1         |
        | Liam       | Wilson    | 2         |
      When I search for students with name containing "Emma"
      Then I see 2 students in the results
      And the results include "Emma Wilson" and "Emma Johnson"

    Scenario: Filter students by school
      Given students exist in multiple schools
      When I filter students by school "Lincoln Elementary"
      Then I only see students enrolled in "Lincoln Elementary"

    Scenario: Filter students by active IEP
      Given students exist with and without IEPs
      When I filter students by "Has Active IEP"
      Then I only see students with has_iep = true
      And IEP end date is in the future

  Rule: Student records have controlled access

    Scenario: View assigned students
      Given I am assigned to students "Emma Wilson" and "Liam Brown"
      When I view my caseload
      Then I see "Emma Wilson" and "Liam Brown"
      And I cannot see unassigned students

    Scenario: View student detail requires assignment
      Given a student "Not My Student" exists
      And I am not assigned to this student
      When I attempt to view the student details
      Then access is denied with error "Not authorized to view this student"
      And an audit log entry is created for "unauthorized_access_attempt"

  Rule: Students can be deactivated but not deleted

    Scenario: Deactivate student who moved
      Given a student "Emma Wilson" exists
      When I deactivate the student with reason "Moved out of district"
      Then the student is marked as inactive
      And the student's data is retained
      And the student does not appear in active searches
      And an audit log entry is created for "student_deactivated"

    Scenario: Reactivate student who returned
      Given an inactive student "Emma Wilson" exists
      When I reactivate the student
      Then the student is marked as active
      And the student appears in active searches
      And an audit log entry is created for "student_reactivated"

  Rule: Parent information must be maintained

    Scenario: Add secondary parent contact
      Given a student "Emma Wilson" exists with one parent contact
      When I add a secondary parent:
        | Field        | Value               |
        | Name         | Robert Wilson       |
        | Phone        | 555-0456           |
        | Email        | rob.wilson@email.com |
        | Relationship | Father              |
      Then the parent contact is added
      And both parents can access the parent portal

    Scenario: Update parent contact information
      Given a student has parent "Lisa Wilson" with phone "555-0123"
      When I update the parent's phone to "555-9999"
      Then the parent's phone is updated
      And an audit log entry is created for "parent_contact_updated"

  Rule: Student history must be preserved

    Scenario: View student's service history
      Given a student "Emma Wilson" has received services for 2 years
      When I view the student's history
      Then I see all past services
      And I see all past therapists
      And I see all past goals
      And I see all past evaluations

    Scenario: Transfer student between schools
      Given a student "Emma Wilson" is enrolled at "Lincoln Elementary"
      When I transfer the student to "Washington Middle"
      Then the student's current school is "Washington Middle"
      And the student's history at "Lincoln Elementary" is preserved
      And an audit log entry is created for "student_transferred"