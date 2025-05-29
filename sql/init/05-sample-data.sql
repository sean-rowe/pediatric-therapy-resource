-- =====================================================
-- SAMPLE DATA FOR TESTING
-- =====================================================

USE TherapyDocs;
GO

-- Insert sample therapist
INSERT INTO users (email, password_hash, first_name, last_name, service_type, license_number, license_state) 
VALUES ('therapist@example.com', '$2b$12$sample_hash', 'Sarah', 'Johnson', 'occupational_therapy', 'OT12345', 'TX');

-- Insert sample school
INSERT INTO schools (name, district, address, phone, principal_name)
VALUES ('Lincoln Elementary', 'Springfield ISD', '123 School St, Springfield, TX', '555-0123', 'Dr. Smith');

-- Insert sample students
DECLARE @school_id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM schools);
DECLARE @therapist_id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users);

INSERT INTO students (first_name, last_name, date_of_birth, grade_level, school_id, student_id, has_iep, primary_disability, medical_alerts, parent_name, parent_phone)
VALUES 
('Emma', 'Wilson', '2015-03-15', '3rd', @school_id, 'SW12345', 1, 'Developmental Delay', 'Uses hearing aids', 'Lisa Wilson', '555-0456'),
('Liam', 'Brown', '2016-07-22', '2nd', @school_id, 'LB67890', 1, 'Autism Spectrum Disorder', 'Sensory sensitivities', 'Michael Brown', '555-0789');

-- Insert sample IEP goals
DECLARE @student1_id UNIQUEIDENTIFIER = (SELECT id FROM students WHERE student_id = 'SW12345');
DECLARE @student2_id UNIQUEIDENTIFIER = (SELECT id FROM students WHERE student_id = 'LB67890');

INSERT INTO iep_goals (student_id, goal_number, goal_area, goal_text, measurement_method, baseline, target_date, responsible_therapist_id)
VALUES 
(@student1_id, 'Goal 1', 'Fine Motor', 'Emma will improve handwriting legibility to 80% accuracy when copying sentences', '4 out of 5 trials', 'Currently 40% legible', '2025-03-15', @therapist_id),
(@student1_id, 'Goal 2', 'Visual Motor', 'Emma will complete age-appropriate cutting activities independently', 'Independently in 3 consecutive sessions', 'Requires moderate assistance', '2025-03-15', @therapist_id),
(@student2_id, 'Goal 1', 'Sensory Processing', 'Liam will tolerate tactile activities for 10 minutes without behavioral disruption', '80% of sessions', 'Currently tolerates 2-3 minutes', '2025-07-22', @therapist_id);

-- Insert sample services
INSERT INTO services (student_id, therapist_id, school_id, service_type, minutes_per_week, frequency_per_week, service_location, start_date)
VALUES 
(@student1_id, @therapist_id, @school_id, 'occupational_therapy', 30, 2, 'OT Room', '2024-08-15'),
(@student2_id, @therapist_id, @school_id, 'occupational_therapy', 45, 2, 'General Ed Classroom', '2024-08-15');

-- Insert sample content types
INSERT INTO content_library (title, description, content_type, service_type, skill_areas, age_range_min, age_range_max, difficulty_level, tags, is_ai_generated)
VALUES 
('Fine Motor Maze - Circles', 'Maze focusing on circular motions for fine motor development', 'maze', 'occupational_therapy', '["fine_motor", "bilateral_coordination"]', 5, 8, 2, '["maze", "fine_motor", "circles"]', 0),
('Handwriting Practice - Letters A-E', 'Coloring sheet with letter tracing', 'coloring_sheet', 'occupational_therapy', '["handwriting", "visual_motor"]', 4, 7, 1, '["handwriting", "letters", "tracing"]', 0),
('Sensory Break Cards', 'Visual cards for sensory break activities', 'worksheet', 'occupational_therapy', '["sensory_processing", "self_regulation"]', 5, 12, 1, '["sensory", "breaks", "regulation"]', 0);

-- Insert sample evaluation template
INSERT INTO evaluation_templates (assessment_name, assessment_type, service_type, age_range_min, age_range_max, estimated_time_minutes, sections, scoring_criteria)
VALUES (
    'Fine Motor Skills Assessment',
    'informal',
    'occupational_therapy',
    3,
    10,
    45,
    '[
        {"name": "Grasping Skills", "items": ["pencil_grasp", "pincer_grasp", "lateral_pinch"]},
        {"name": "Bilateral Coordination", "items": ["cutting_skills", "bead_stringing", "paper_folding"]},
        {"name": "Visual Motor", "items": ["copying_shapes", "drawing_person", "maze_completion"]}
    ]',
    '{
        "pencil_grasp": {"max_score": 3, "criteria": "3=Mature, 2=Developing, 1=Immature"},
        "cutting_skills": {"max_score": 5, "criteria": "5=Cuts on line, 1=Unable to cut"}
    }'
);

-- Insert sample note templates
INSERT INTO note_templates (name, service_type, template_type, subjective_template, objective_template, assessment_template, plan_template, created_by_user_id)
VALUES (
    'OT SOAP Template',
    'occupational_therapy',
    'soap',
    'Student reported [feeling/state]. Teacher noted [observations].',
    'Student participated in [activities]. Demonstrated [skills/behaviors]. Required [assistance level].',
    'Student showed [progress level] toward goals. Areas of strength: [strengths]. Areas needing support: [challenges].',
    'Continue working on [goals]. Next session will focus on [activities]. Home program: [recommendations].',
    @therapist_id
);

GO

PRINT 'Sample data inserted successfully!';
GO