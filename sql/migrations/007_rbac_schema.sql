-- RBAC (Role-Based Access Control) Schema Migration
-- Creates tables for roles, permissions, role_permissions, and user_roles

-- Create roles table
CREATE TABLE roles (
    id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    display_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    role_type NVARCHAR(50) NOT NULL CHECK (role_type IN ('system', 'organization', 'custom')),
    organization_id UNIQUEIDENTIFIER NULL REFERENCES organizations(id) ON DELETE CASCADE,
    parent_role_id UNIQUEIDENTIFIER NULL REFERENCES roles(id),
    is_active BIT DEFAULT 1,
    created_by UNIQUEIDENTIFIER NULL REFERENCES users(id),
    created_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    updated_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_roles_name_organization UNIQUE(name, organization_id)
);

-- Create permissions table
CREATE TABLE permissions (
    id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    display_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    resource NVARCHAR(100) NOT NULL,
    action NVARCHAR(50) NOT NULL,
    scope NVARCHAR(20) NOT NULL CHECK (scope IN ('own', 'team', 'organization', 'all')),
    is_system BIT DEFAULT 0,
    created_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    updated_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_permissions_resource_action_scope UNIQUE(resource, action, scope)
);

-- Create role_permissions junction table
CREATE TABLE role_permissions (
    id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    role_id UNIQUEIDENTIFIER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UNIQUEIDENTIFIER NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    granted_by UNIQUEIDENTIFIER NULL REFERENCES users(id),
    granted_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_role_permissions_role_permission UNIQUE(role_id, permission_id)
);

-- Create user_roles junction table
CREATE TABLE user_roles (
    id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    user_id UNIQUEIDENTIFIER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UNIQUEIDENTIFIER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    organization_id UNIQUEIDENTIFIER NULL REFERENCES organizations(id) ON DELETE CASCADE,
    assigned_by UNIQUEIDENTIFIER NULL REFERENCES users(id),
    assigned_at DATETIMEOFFSET DEFAULT GETUTCDATE(),
    expires_at DATETIMEOFFSET NULL,
    is_active BIT DEFAULT 1,
    CONSTRAINT UQ_user_roles_user_role_org UNIQUE(user_id, role_id, organization_id)
);

-- Create indexes for performance
CREATE INDEX IX_roles_organization_id ON roles(organization_id);
CREATE INDEX IX_roles_parent_role_id ON roles(parent_role_id);
CREATE INDEX IX_roles_role_type ON roles(role_type);
CREATE INDEX IX_roles_is_active ON roles(is_active);

CREATE INDEX IX_permissions_resource ON permissions(resource);
CREATE INDEX IX_permissions_action ON permissions(action);
CREATE INDEX IX_permissions_scope ON permissions(scope);
CREATE INDEX IX_permissions_is_system ON permissions(is_system);

CREATE INDEX IX_role_permissions_role_id ON role_permissions(role_id);
CREATE INDEX IX_role_permissions_permission_id ON role_permissions(permission_id);

CREATE INDEX IX_user_roles_user_id ON user_roles(user_id);
CREATE INDEX IX_user_roles_role_id ON user_roles(role_id);
CREATE INDEX IX_user_roles_organization_id ON user_roles(organization_id);
CREATE INDEX IX_user_roles_is_active ON user_roles(is_active);
CREATE INDEX IX_user_roles_expires_at ON user_roles(expires_at);

-- Insert system roles
INSERT INTO roles (id, name, display_name, description, role_type, is_active, created_at, updated_at)
VALUES 
    (NEWID(), 'super_admin', 'Super Administrator', 'Full system access with all permissions', 'system', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'org_admin', 'Organization Administrator', 'Full access within organization', 'system', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'therapist', 'Therapist', 'Standard therapist access to patient records and treatment plans', 'system', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patient', 'Patient', 'Patient access to own records and progress', 'system', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'viewer', 'Viewer', 'Read-only access to assigned records', 'system', 1, GETUTCDATE(), GETUTCDATE());

-- Insert core permissions
INSERT INTO permissions (id, name, display_name, description, resource, action, scope, is_system, created_at, updated_at)
VALUES 
    -- User management permissions
    (NEWID(), 'users.create.organization', 'Create Users (Organization)', 'Create new users within organization', 'users', 'create', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'users.read.own', 'Read Own Profile', 'View own user profile', 'users', 'read', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'users.read.organization', 'Read Users (Organization)', 'View users within organization', 'users', 'read', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'users.update.own', 'Update Own Profile', 'Update own user profile', 'users', 'update', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'users.update.organization', 'Update Users (Organization)', 'Update users within organization', 'users', 'update', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'users.delete.organization', 'Delete Users (Organization)', 'Delete users within organization', 'users', 'delete', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    
    -- Patient management permissions
    (NEWID(), 'patients.create.organization', 'Create Patients', 'Create new patient records', 'patients', 'create', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.read.own', 'Read Own Records', 'View own patient records', 'patients', 'read', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.read.team', 'Read Team Patients', 'View patients assigned to team', 'patients', 'read', 'team', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.read.organization', 'Read All Patients', 'View all patients in organization', 'patients', 'read', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.update.own', 'Update Own Records', 'Update own patient records', 'patients', 'update', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.update.team', 'Update Team Patients', 'Update patients assigned to team', 'patients', 'update', 'team', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'patients.update.organization', 'Update All Patients', 'Update all patients in organization', 'patients', 'update', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    
    -- Treatment plan permissions
    (NEWID(), 'treatments.create.team', 'Create Treatment Plans', 'Create new treatment plans', 'treatments', 'create', 'team', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'treatments.read.own', 'Read Own Treatments', 'View own treatment plans', 'treatments', 'read', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'treatments.read.team', 'Read Team Treatments', 'View team treatment plans', 'treatments', 'read', 'team', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'treatments.update.own', 'Update Own Treatments', 'Update own treatment plans', 'treatments', 'update', 'own', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'treatments.update.team', 'Update Team Treatments', 'Update team treatment plans', 'treatments', 'update', 'team', 1, GETUTCDATE(), GETUTCDATE()),
    
    -- Role management permissions
    (NEWID(), 'roles.create.organization', 'Create Roles', 'Create custom roles within organization', 'roles', 'create', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'roles.read.organization', 'Read Roles', 'View roles within organization', 'roles', 'read', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'roles.update.organization', 'Update Roles', 'Update roles within organization', 'roles', 'update', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'roles.delete.organization', 'Delete Roles', 'Delete custom roles within organization', 'roles', 'delete', 'organization', 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'roles.assign.organization', 'Assign Roles', 'Assign roles to users within organization', 'roles', 'assign', 'organization', 1, GETUTCDATE(), GETUTCDATE());