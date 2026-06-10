-- Seed demo user passwords.
-- Hashes are bcrypt (work factor 12) of the demo passwords:
--   Morgan Ellis  →  demo@Admin1
--   Priya Shah    →  demo@Disp1
--   Darius Cole   →  demo@Yard1
--
-- Regenerate: BCrypt.Net.BCrypt.HashPassword("<password>", 12)

UPDATE dbo.Users
SET PasswordHash = '$2a$12$MFLfkBBc6cuy86Kn8CMFq.niD5ajgkItCIUs/9EZ1jvMZq2Fd0hVS'
WHERE EmailAddress = 'morgan.ellis@atlasmeridian.example'
  AND TenantId = 'tenant-demo';

UPDATE dbo.Users
SET PasswordHash = '$2a$12$ZWlwwEtOnQuTjNRlq8npI.JoU5gWi9c8c4hIUYM.PB6RtqXwBsEDW'
WHERE EmailAddress = 'priya.shah@atlasmeridian.example'
  AND TenantId = 'tenant-demo';

UPDATE dbo.Users
SET PasswordHash = '$2a$12$REhXC21XJV1pbM7oVDv40OdWSWEEt.ItL8yGoyA7bwaMRZ46zkj8O'
WHERE EmailAddress = 'darius.cole@atlasmeridian.example'
  AND TenantId = 'tenant-demo';
