# TherapyDocs Requirements Specification

## Functional Requirements

### Authentication & User Management
1. Therapists must register with email, password, name, service type, and optional license information
2. Users must authenticate with email and password to receive JWT tokens
3. System must support password reset via email with secure tokens
4. User sessions must expire after 30 minutes of inactivity
5. System must support multiple user roles (therapist, admin)

### Student Management
6. Therapists must create student profiles with demographics and IEP information
7. System must track student medical alerts and behavioral notes
8. Students must be associated with schools and assigned therapists
9. System must support student search by name, ID, or school
10. Student records must include parent/guardian contact information

### IEP Goal Management
11. Therapists must create measurable IEP goals for students
12. Goals must include measurement methods, baselines, and target dates
13. System must track goal progress with dated entries
14. Goals must support status tracking (active, met, discontinued)
15. System must generate goal progress reports

### Service Documentation
16. Therapists must schedule therapy sessions with students
17. System must support recurring appointment scheduling
18. Therapists must document sessions using SOAP format
19. Documentation must support offline creation with sync
20. System must track session duration and attendance

### Content Generation & Library
21. System must generate therapy materials using AI (mazes, worksheets)
22. Generated content must be age and skill appropriate
23. Users must rate content effectiveness
24. System must recommend content based on student needs
25. Content library must support search and filtering

### Digital Evaluations
26. Therapists must conduct standardized evaluations digitally
27. System must auto-score evaluation items
28. Evaluations must generate comprehensive reports
29. System must track evaluation history
30. Reports must include interpretation and recommendations

### Billing & Insurance
31. System must track insurance information for students
32. Therapists must create claims for services rendered
33. System must validate CPT codes and modifiers
34. Claims must support batch submission
35. System must track payments and denials
36. Denial management must include appeals workflow

### Compliance & Reporting
37. System must maintain HIPAA-compliant audit logs
38. FERPA consents must be tracked and enforced
39. Data sharing must be logged with justification
40. System must generate state-specific compliance reports
41. Security incidents must be tracked and reported

### Parent Portal
42. Parents must access student progress reports
43. Parents must communicate with therapists securely
44. System must support document sharing with parents
45. Parent notifications must be multilingual
46. Access must be controlled by consent status

### Teletherapy Support
47. System must track virtual session details
48. Virtual resources must be categorized and searchable
49. Session quality metrics must be recorded
50. Platform integrations must be supported

## Non-Functional Requirements

### Performance
51. Page load time must be <2 seconds on 4G networks
52. API response time must be <200ms for read operations
53. System must support 10,000 concurrent users
54. Database queries must complete in <100ms
55. Offline sync must complete in <30 seconds

### Security
56. All data must be encrypted at rest (AES-256)
57. All communications must use TLS 1.3+
58. Passwords must meet complexity requirements
59. System must support MFA for admin accounts
60. API must implement rate limiting (100 req/min)

### Availability
61. System uptime must be 99.9% (excluding maintenance)
62. Database backups must occur every 4 hours
63. Recovery time objective (RTO) must be <4 hours
64. Recovery point objective (RPO) must be <1 hour
65. System must support zero-downtime deployments

### Scalability
66. System must scale horizontally for API layer
67. Database must support read replicas
68. Storage must auto-scale for documents
69. System must handle 100% YoY growth
70. Performance must not degrade with data growth

### Usability
71. Mobile UI must be touch-optimized
72. System must work offline for core functions
73. UI must support accessibility standards (WCAG 2.1 AA)
74. System must provide contextual help
75. Error messages must be user-friendly

### Integration
76. System must integrate with PowerSchool via API
77. System must integrate with Clever for SSO
78. System must export data in standard formats
79. System must support webhook notifications
80. API must be RESTful and documented

## Data Entities and Relationships

### Core Entities
- Users (therapists, admins)
- Schools
- Students  
- IEP Goals
- Services
- Appointments
- Content Library
- Evaluations
- Insurance/Billing
- Parents

### Key Relationships
- Users → Many Services → Many Students
- Students → Many Goals → Many Progress Entries
- Students → Many Appointments → Documentation
- Appointments → Content Used → Ratings
- Students → Insurance → Claims → Payments

## Business Rules

81. Email addresses must be unique across all users
82. Students can only have one active IEP at a time
83. Goals must have measurable criteria
84. Sessions cannot be documented after 7 days
85. Claims must be submitted within timely filing limits
86. Passwords expire every 90 days
87. Inactive accounts are disabled after 180 days
88. Consent must be obtained before sharing data
89. Evaluations require physician orders in some states
90. Service authorizations cannot be exceeded

## System Boundaries

### In Scope
- Web application (responsive)
- iOS/Android apps (React Native)
- REST API
- SQL Server database
- Document storage (S3-compatible)
- Email notifications

### Out of Scope
- Native mobile apps
- Video conferencing (use external)
- Payment processing (use external)
- SMS notifications (phase 2)
- AI model training (use external APIs)

## Integration Points

### External Systems
- Student Information Systems (SIS)
  - PowerSchool
  - Infinite Campus
  - Skyward
- Identity Providers
  - Clever
  - ClassLink
  - Google Workspace
- Payment/Billing
  - Stripe
  - Medicaid clearinghouses
- AI Services
  - OpenAI API
  - Azure Cognitive Services
- Communication
  - SendGrid (email)
  - Twilio (future SMS)