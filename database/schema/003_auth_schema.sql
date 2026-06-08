-- Add PasswordHash column to Users for database-backed authentication.
-- The in-memory auth service uses SHA-256 hex hashes; this mirrors that scheme.
ALTER TABLE dbo.Users
ADD PasswordHash NVARCHAR(200) NULL;

CREATE INDEX IX_Users_TenantId_EmailAddress ON dbo.Users (TenantId, EmailAddress);
