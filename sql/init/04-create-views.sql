-- =====================================================
-- VIEWS FOR REPORTING AND ANALYTICS
-- =====================================================

USE TherapyDocs;
GO

-- Daily Schedule View
CREATE VIEW daily_schedule AS
SELECT 
    a.id,
    a.scheduled_date,
    a.scheduled_start_time,
    a.scheduled_end_time,
    a.status,
    s.first_name + ' ' + s.last_name as student_name,
    s.grade_level,
    sch.name as school_name,
    a.location,
    srv.service_type,
    CASE WHEN a.subjective IS NOT NULL THEN 1 ELSE 0 END as has_documentation
FROM appointments a
JOIN students s ON a.student_id = s.id
JOIN schools sch ON a.school_id = sch.id
JOIN services srv ON a.service_id = srv.id
WHERE a.scheduled_date >= DATEADD(day, -7, GETDATE())
    AND a.scheduled_date <= DATEADD(day, 30, GETDATE());
GO

-- Student Caseload Summary View
CREATE VIEW student_caseload AS
SELECT 
    s.id,
    s.first_name + ' ' + s.last_name as student_name,
    s.grade_level,
    s.date_of_birth,
    sch.name as school_name,
    srv.service_type,
    srv.minutes_per_week,
    srv.frequency_per_week,
    (SELECT COUNT(*) FROM iep_goals g WHERE g.student_id = s.id AND g.status = 'active') as active_goals,
    (SELECT MAX(scheduled_date) FROM appointments a WHERE a.student_id = s.id AND a.status = 'completed') as last_session_date,
    s.medical_alerts,
    s.behavioral_notes
FROM students s
JOIN services srv ON s.id = srv.student_id AND srv.status = 'active'
JOIN schools sch ON s.school_id = sch.id
WHERE s.is_active = 1;
GO

-- Goal Progress Summary View
CREATE VIEW goal_progress_summary AS
SELECT 
    g.id,
    g.goal_number,
    g.goal_area,
    g.goal_text,
    g.status,
    s.first_name + ' ' + s.last_name as student_name,
    COUNT(gp.id) as progress_entries,
    AVG(CAST(gp.progress_rating as FLOAT)) as avg_progress_rating,
    MAX(gp.date_recorded) as last_progress_date,
    g.current_performance
FROM iep_goals g
JOIN students s ON g.student_id = s.id
LEFT JOIN goal_progress_entries gp ON g.id = gp.goal_id
WHERE g.is_active = 1
GROUP BY g.id, g.goal_number, g.goal_area, g.goal_text, g.status,
         s.first_name, s.last_name, g.current_performance;
GO

-- Content Recommendations View
CREATE VIEW content_recommendations AS
SELECT 
    cl.id,
    cl.title,
    cl.description,
    cl.content_type,
    cl.service_type,
    cl.age_range_min,
    cl.age_range_max,
    cl.difficulty_level,
    cl.file_url,
    cl.thumbnail_url,
    cl.is_ai_generated,
    cl.download_count,
    cl.tags,
    AVG(CAST(cr.overall_rating as FLOAT)) as avg_rating,
    COUNT(cr.id) as rating_count
FROM content_library cl
LEFT JOIN content_ratings cr ON cl.id = cr.content_id
WHERE cl.is_active = 1
GROUP BY cl.id, cl.title, cl.description, cl.content_type, cl.service_type,
         cl.age_range_min, cl.age_range_max, cl.difficulty_level, cl.file_url,
         cl.thumbnail_url, cl.is_ai_generated, cl.download_count, cl.tags;
GO

-- Evaluation Schedule View
CREATE VIEW evaluation_schedule AS
SELECT 
    s.id as student_id,
    s.first_name + ' ' + s.last_name as student_name,
    s.next_evaluation_due,
    s.last_evaluation_date,
    srv.therapist_id,
    u.first_name + ' ' + u.last_name as therapist_name,
    CASE 
        WHEN s.next_evaluation_due <= CAST(GETDATE() AS DATE) THEN 'overdue'
        WHEN s.next_evaluation_due <= DATEADD(day, 30, CAST(GETDATE() AS DATE)) THEN 'due_soon'
        ELSE 'future'
    END as urgency_level
FROM students s
JOIN services srv ON s.id = srv.student_id AND srv.status = 'active'
JOIN users u ON srv.therapist_id = u.id
WHERE s.is_active = 1
    AND s.has_iep = 1
    AND s.next_evaluation_due IS NOT NULL;
GO

-- Content Usage Analytics View
CREATE VIEW content_usage_stats AS
SELECT 
    cl.id,
    cl.title,
    cl.content_type,
    cl.service_type,
    cl.download_count,
    AVG(CAST(cr.overall_rating as FLOAT)) as avg_rating,
    COUNT(DISTINCT cr.rated_by_user_id) as unique_raters,
    COUNT(DISTINCT cr.appointment_id) as sessions_used_in
FROM content_library cl
LEFT JOIN content_ratings cr ON cl.id = cr.content_id  
GROUP BY cl.id, cl.title, cl.content_type, cl.service_type, cl.download_count;
GO