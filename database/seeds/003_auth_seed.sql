-- Seed demo user passwords.
-- Hashes are SHA-256 (lowercase hex) of the demo passwords:
--   Morgan Ellis  →  demo@Admin1
--   Priya Shah    →  demo@Disp1
--   Darius Cole   →  demo@Yard1
--
-- Regenerate:  printf '%s' "<password>" | shasum -a 256

UPDATE dbo.Users
SET PasswordHash = 'd90bd915b31118219874647d0ebaef274a359fad528dc0942ecd3506b9cc039d'
WHERE EmailAddress = 'morgan.ellis@atlasmeridian.example'
  AND TenantId = 'tenant-demo';

UPDATE dbo.Users
SET PasswordHash = '22c1fafcc463eb1247aef9914baaa1dae0a985a2979fe63ae8327224f4ba928c'
WHERE EmailAddress = 'priya.shah@atlasmeridian.example'
  AND TenantId = 'tenant-demo';

UPDATE dbo.Users
SET PasswordHash = '824894a8891f45a5ae560d0292986b2f9ee09f764a4b08dfb6b90e447cded838'
WHERE EmailAddress = 'darius.cole@atlasmeridian.example'
  AND TenantId = 'tenant-demo';
