INSERT INTO dbo.TenantSettings (TenantId, SettingKey, SettingValue)
VALUES
    ('tenant-demo', 'Tenant header mapping', 'X-Tenant-Id'),
    ('tenant-demo', 'Default operational region', 'Pacific'),
    ('tenant-demo', 'Brand profile', 'Atlas Meridian Logistics');

INSERT INTO dbo.DispatchRules (TenantId, RuleName, RuleValue, IsEnabled)
VALUES
    ('tenant-demo', 'Auto-flag delays after', '12 minutes', 1),
    ('tenant-demo', 'Recompute dock assignment every', '90 seconds', 1),
    ('tenant-demo', 'Escalate missed departure after', '2 failed retries', 1);
