CREATE TABLE dbo.TenantSettings (
    TenantSettingId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(250) NOT NULL,
    CONSTRAINT FK_TenantSettings_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_TenantSettings_TenantId_SettingKey UNIQUE (TenantId, SettingKey)
);

CREATE TABLE dbo.DispatchRules (
    DispatchRuleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    RuleName NVARCHAR(120) NOT NULL,
    RuleValue NVARCHAR(250) NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_DispatchRules_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE INDEX IX_TenantSettings_TenantId ON dbo.TenantSettings (TenantId);
CREATE INDEX IX_DispatchRules_TenantId ON dbo.DispatchRules (TenantId);
