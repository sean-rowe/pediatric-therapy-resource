-- =====================================================
-- ADDITIONAL PERFORMANCE INDEXES
-- =====================================================

USE TherapyDocs;
GO

-- Additional indexes for existing tables
-- Billing and Insurance Indexes
CREATE INDEX idx_billing_claims_payer ON billing_claims(payer_id, claim_status);
CREATE INDEX idx_billing_claims_date ON billing_claims(service_date);
CREATE INDEX idx_claim_line_items_claim ON claim_line_items(claim_id);
CREATE INDEX idx_payment_transactions_claim ON payment_transactions(claim_id);
CREATE INDEX idx_payment_batches_date ON payment_batches(batch_date);
CREATE INDEX idx_service_authorizations_student_dates ON service_authorizations(student_id, auth_start_date, auth_end_date);

-- Session Management Indexes
CREATE INDEX idx_virtual_sessions_therapist ON virtual_sessions(therapist_id, session_date);
CREATE INDEX idx_virtual_sessions_student ON virtual_sessions(student_id, session_date);
CREATE INDEX idx_group_sessions_therapist ON group_sessions(therapist_id, session_date);
CREATE INDEX idx_group_session_students_session ON group_session_students(session_id);
CREATE INDEX idx_group_session_students_student ON group_session_students(student_id);

-- Caseload Management Indexes
CREATE INDEX idx_therapist_caseloads_therapist ON therapist_caseloads(therapist_id, is_active);
CREATE INDEX idx_therapist_caseloads_student ON therapist_caseloads(student_id, is_active);
CREATE INDEX idx_caseload_analytics_therapist ON caseload_analytics(therapist_id, analysis_date);

-- Document Management Indexes
CREATE INDEX idx_documents_student ON documents(student_id, document_type);
CREATE INDEX idx_documents_date ON documents(created_date);
CREATE INDEX idx_discharge_summaries_student ON discharge_summaries(student_id);

-- Parent Portal Indexes
CREATE INDEX idx_parent_accounts_email ON parent_accounts(email);
CREATE INDEX idx_parent_student_access_parent ON parent_student_access(parent_id);
CREATE INDEX idx_parent_student_access_student ON parent_student_access(student_id);
CREATE INDEX idx_parent_communications_parent ON parent_communications(parent_id, communication_date);

-- Compliance and Reporting Indexes
CREATE INDEX idx_compliance_audits_date ON compliance_audits(audit_date, audit_type);
CREATE INDEX idx_state_report_submissions_type_date ON state_report_submissions(report_type, submission_date);
CREATE INDEX idx_hipaa_disclosures_student ON hipaa_disclosures(student_id, disclosure_date);
CREATE INDEX idx_ferpa_consents_student ON ferpa_consents(student_id);

-- Referral and Outcome Indexes
CREATE INDEX idx_referrals_student ON referrals(student_id, referral_date);
CREATE INDEX idx_referrals_source ON referrals(referral_source_id);
CREATE INDEX idx_student_outcome_scores_student ON student_outcome_scores(student_id, assessment_date);
CREATE INDEX idx_outcome_measures_active ON outcome_measures(is_active);

-- Indexes for new tables from file 10
CREATE INDEX idx_content_generation_prompts_type ON content_generation_prompts(content_type, is_active);
CREATE INDEX idx_session_materials_session ON session_materials(session_id);
CREATE INDEX idx_teletherapy_logs_session ON teletherapy_logs(session_id, event_timestamp);
CREATE INDEX idx_parent_portal_activity_parent_date ON parent_portal_activity(parent_id, created_at);
CREATE INDEX idx_compliance_training_user_type ON compliance_training(user_id, training_type);
CREATE INDEX idx_compliance_training_expiration ON compliance_training(expiration_date);
CREATE INDEX idx_caseload_transfers_student_date ON caseload_transfers(student_id, transfer_date);
CREATE INDEX idx_report_schedules_active ON report_schedules(is_active, next_run_date);
CREATE INDEX idx_backup_history_date ON backup_history(created_at, backup_type);
CREATE INDEX idx_archive_records_table ON archive_records(table_name, archived_at);

-- Composite indexes for complex queries
CREATE INDEX idx_therapy_composite ON appointments(therapist_id, student_id, scheduled_date, status);
CREATE INDEX idx_billing_composite ON billing_claims(payer_id, claim_status, service_date, student_id);
CREATE INDEX idx_auth_composite ON service_authorizations(student_id, auth_status, auth_end_date);

-- Indexes for text search (if full-text search is enabled)
-- CREATE FULLTEXT INDEX idx_ft_session_notes ON session_notes(notes);
-- CREATE FULLTEXT INDEX idx_ft_documents ON documents(document_name, description);
-- CREATE FULLTEXT INDEX idx_ft_iep_goals ON iep_goals(goal_text);

GO