Feature: System Administration
  As a system administrator
  I want to configure and maintain the therapy documentation system
  So that it operates efficiently and meets organizational needs

  Background:
    Given I am logged in as system administrator
    And I have full administrative privileges
    And the system has been operational for 6 months

  Rule: User management must be secure and efficient

    Scenario: Onboard new therapist
      Given a new therapist "Jennifer Martinez" is hired
      When I create her user account with:
        | Field                | Value                        |
        | Email               | jmartinez@therapy.com        |
        | Role                | Therapist                    |
        | Service Type        | Occupational Therapy         |
        | License Number      | OT-98765                     |
        | License State       | TX                           |
        | Supervisor          | Sarah Johnson                |
        | Start Date          | 2024-02-01                   |
        | Schools Assigned    | Lincoln, Washington          |
      Then the system:
        | Action              | Result                       |
        | Creates account     | With temporary password      |
        | Sends welcome email | With setup instructions      |
        | Assigns permissions | Based on therapist role      |
        | Creates audit entry | Account creation logged      |
        | Notifies supervisor | New supervisee alert         |
      And account requires password change on first login

    Scenario: Manage user roles and permissions
      Given multiple user roles exist
      When I review role permissions
      Then I can configure:
        | Role               | Permissions                                      |
        | Therapist         | View own caseload, document sessions, run reports |
        | Supervisor        | Above + view team, approve documents, reassign   |
        | Administrator     | Above + user management, billing, configuration   |
        | Billing Specialist| View billing, submit claims, post payments       |
        | Executive         | View all reports, no PHI access, financial only  |
      And changes require second administrator approval

    Scenario: Deactivate departing user
      Given "Former Employee" is leaving
      When I process termination on "2024-01-31"
      Then system executes:
        | Step               | Action                                    |
        | Access Removal    | Immediate deactivation at 5:00 PM        |
        | Caseload Transfer | Students reassigned per plan             |
        | Document Retention| All records retained, access restricted  |
        | License Release   | Removed from active license count        |
        | Audit Trail       | Complete access history archived         |
        | Final Reports     | Productivity and compliance summary      |

  Rule: System configuration must be flexible

    Scenario: Configure district-specific requirements
      Given "Springfield ISD" has unique needs
      When I create district configuration
      Then I can customize:
        | Setting                  | Configuration                   |
        | Session Duration Options | 15, 30, 45, 60 minutes         |
        | Required Documentation  | Additional behavior notes       |
        | Billing Codes          | District-specific codes         |
        | Report Templates       | Custom monthly format           |
        | Approval Workflows     | Principal sign-off required     |
        | Data Retention        | 10 years (exceeds default)      |
      And settings apply only to that district's students

    Scenario: Set up automated workflows
      Given repetitive tasks need automation
      When I configure workflow for "session documentation reminders"
      Then I set:
        | Trigger              | Condition                      | Action                    |
        | Session completed   | No documentation after 24hr    | Email therapist          |
        | Still pending       | No documentation after 36hr    | Email + SMS              |
        | Approaching deadline| 6 hours until 48hr limit      | Email + supervisor CC    |
        | Overdue            | Past 48 hours                  | Lock billing + escalate  |
      And test mode available before activation

    Scenario: Manage integration settings
      Given external systems need connection
      When I configure integrations
      Then I can set up:
        | System              | Integration Type    | Configuration            |
        | School SIS         | REST API           | API key, endpoints       |
        | Billing Clearinghouse | SFTP            | Credentials, schedule    |
        | Email Server       | SMTP               | Server, port, TLS        |
        | Calendar System    | CalDAV             | URL, sync frequency      |
        | Telehealth Platform| OAuth 2.0          | Client ID, permissions   |
      With connection testing for each

  Rule: System maintenance ensures reliability

    Scenario: Schedule system maintenance
      Given updates need deployment
      When I schedule maintenance window
      Then I configure:
        | Setting              | Value                          |
        | Date/Time           | 2024-02-10 02:00-04:00 EST   |
        | Type                | Database optimization          |
        | Impact              | Read-only mode                 |
        | Notification        | 1 week, 1 day, 1 hour prior   |
        | Rollback Plan       | Automated with manual override |
      And system displays countdown banner
      And emergency contacts documented

    Scenario: Monitor system performance
      Given performance metrics collected
      When I view system health dashboard
      Then I see:
        | Metric               | Current    | Threshold  | Status    |
        | Response Time       | 245ms avg  | <500ms     | Healthy   |
        | Database Load       | 42%        | <80%       | Healthy   |
        | Storage Used        | 1.2TB/2TB  | <90%       | Monitor   |
        | Concurrent Users    | 87         | <200       | Healthy   |
        | Error Rate          | 0.02%      | <0.1%      | Healthy   |
        | Backup Status       | Current    | Daily      | Healthy   |
      With historical trends and alerts

    Scenario: Manage data archival
      Given data retention policies exist
      When I run archival process
      Then system:
        | Data Type           | Action                         | Retention            |
        | Session notes >7yr  | Move to cold storage          | Accessible on request |
        | Audit logs >6yr     | Compress and archive          | Searchable index     |
        | Billing >10yr       | Archive with index            | 48hr retrieval       |
        | Emails >2yr         | Delete after archive          | PDF copies kept      |
      And archival validated before deletion
      And restoration tested quarterly

  Rule: Security configuration protects data

    Scenario: Configure security policies
      Given security requirements defined
      When I set security parameters
      Then I configure:
        | Policy               | Setting                        |
        | Password Complexity | 12+ chars, mixed case, special |
        | Password Expiration | 90 days                       |
        | Session Timeout     | 30 minutes idle               |
        | MFA Requirement     | All users, SMS or app         |
        | Login Attempts      | Lock after 5 failed           |
        | IP Restrictions     | Optional whitelist per role   |
      And changes logged with justification

    Scenario: Review security audit log
      Given security events logged
      When I review security report
      Then I see:
        | Event Type           | Count | Severity | Action Required |
        | Failed Logins       | 47    | Low      | Monitor         |
        | Permission Changes  | 12    | Medium   | Review each     |
        | Data Exports        | 23    | Medium   | Verify authorized |
        | Access from New IP  | 8     | Low      | Verify legitimate |
        | Admin Actions       | 31    | High     | Audit trail      |
      With drill-down to specific events

  Rule: Billing configuration supports revenue

    Scenario: Configure billing rules
      Given multiple payers with different requirements
      When I set up billing automation
      Then I configure:
        | Payer               | Rules                          |
        | Medicaid           | Require auth, 15-min units     |
        | BCBS               | Prior auth for >8 sessions     |
        | School District    | Monthly batch, net 45          |
        | Private Pay        | Payment due at service         |
      And validations prevent incorrect billing

    Scenario: Set up fee schedules
      Given contract negotiations completed
      When I update fee schedules for "2024"
      Then I enter:
        | Service      | CPT Code | Medicaid | BCBS    | Private |
        | OT Eval      | 97165    | $125.00  | $285.00 | $350.00 |
        | OT Treatment | 97530    | $45.00   | $95.00  | $125.00 |
        | PT Eval      | 97161    | $125.00  | $275.00 | $350.00 |
      With effective dates and version control

  Rule: System supports scalability

    Scenario: Add new therapy clinic location
      Given expansion to new location
      When I configure "Northside Therapy Center"
      Then I set up:
        | Component           | Configuration                  |
        | Address            | 123 North Ave, Suite 100      |
        | Time Zone          | CST                           |
        | Tax ID             | Separate EIN                  |
        | Billing Entity     | Northside LLC                 |
        | Staff Assignments  | Therapists by location        |
        | Service Area       | 5 new schools                 |
      And location appears in all relevant dropdowns

    Scenario: Configure multi-tenant architecture
      Given multiple practice entities
      When I set up data segregation
      Then each tenant has:
        | Isolation           | Implementation                 |
        | Database           | Separate schemas              |
        | User Management    | Independent admin             |
        | Billing            | Separate clearinghouse IDs    |
        | Reporting          | No cross-tenant access        |
        | Customization      | Independent configurations    |
      With single sign-on capability

  Rule: Disaster recovery ensures continuity

    Scenario: Test disaster recovery plan
      Given DR plan documented
      When I initiate DR test
      Then process includes:
        | Step                | Validation                     |
        | Backup Restoration | <4 hours to restore           |
        | Failover Test      | Secondary site operational    |
        | Data Integrity     | Checksums match              |
        | Communication      | All stakeholders notified     |
        | Documentation      | Runbook accurate             |
      And lessons learned documented

    Scenario: Configure automated backups
      Given backup requirements defined
      When I set backup parameters
      Then schedule includes:
        | Backup Type         | Frequency   | Retention     | Location           |
        | Database Full      | Daily 2 AM  | 30 days      | Primary + offsite  |
        | Database Incremental| Hourly      | 7 days       | Primary + offsite  |
        | File Storage       | Daily 3 AM  | 30 days      | Cloud + local      |
        | Configuration      | On change   | All versions | Version control    |
      With automated testing and alerts