-- =====================================================
-- INDEXES FOR PERFORMANCE
-- =====================================================

USE TherapyDocs;
GO

-- User indexes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_service_type ON users(service_type);

-- Student indexes  
CREATE INDEX idx_students_school ON students(school_id);
CREATE INDEX idx_students_name ON students(last_name, first_name);
CREATE INDEX idx_students_active ON students(is_active);

-- Service indexes
CREATE INDEX idx_services_therapist ON services(therapist_id, status);
CREATE INDEX idx_services_student ON services(student_id, status);

-- Appointment indexes
CREATE INDEX idx_appointments_therapist_date ON appointments(therapist_id, scheduled_date);
CREATE INDEX idx_appointments_student ON appointments(student_id);
CREATE INDEX idx_appointments_status ON appointments(status);
CREATE INDEX idx_appointments_sync ON appointments(synced_to_server);

-- Goal indexes
CREATE INDEX idx_goals_student ON iep_goals(student_id, status);
CREATE INDEX idx_goals_therapist ON iep_goals(responsible_therapist_id);
CREATE INDEX idx_goal_progress_goal ON goal_progress_entries(goal_id, date_recorded);

-- Content library indexes
CREATE INDEX idx_content_library_type ON content_library(content_type, service_type);
CREATE INDEX idx_content_library_age ON content_library(age_range_min, age_range_max);

-- Content generation indexes
CREATE INDEX idx_content_requests_user ON content_requests(requested_by_user_id, request_status);
CREATE INDEX idx_content_requests_date ON content_requests(created_at);

-- Evaluation indexes
CREATE INDEX idx_evaluations_student ON evaluations(student_id, evaluation_date);
CREATE INDEX idx_evaluations_therapist ON evaluations(therapist_id, status);
CREATE INDEX idx_evaluations_template ON evaluations(template_id);
CREATE INDEX idx_evaluation_items_eval ON evaluation_items(evaluation_id);

-- Rating indexes
CREATE INDEX idx_content_ratings_content ON content_ratings(content_id);
CREATE INDEX idx_content_ratings_user ON content_ratings(rated_by_user_id);

-- Audit index
CREATE INDEX idx_audit_user_date ON audit_log(user_id, timestamp);
CREATE INDEX idx_audit_student ON audit_log(student_id, timestamp);

GO