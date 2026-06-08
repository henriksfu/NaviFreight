CREATE TABLE dbo.Tenants (
    TenantId NVARCHAR(50) NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Yards (
    YardId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    YardName NVARCHAR(150) NOT NULL,
    Capacity INT NOT NULL,
    OccupiedSlots INT NOT NULL DEFAULT 0,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Yards_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE TABLE dbo.Vehicles (
    VehicleId NVARCHAR(50) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    DriverName NVARCHAR(150) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CurrentYardId INT NULL,
    LastUpdatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Vehicles_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT FK_Vehicles_Yards FOREIGN KEY (CurrentYardId) REFERENCES dbo.Yards(YardId)
);

CREATE INDEX IX_Yards_TenantId ON dbo.Yards (TenantId);
CREATE INDEX IX_Vehicles_TenantId_Status ON dbo.Vehicles (TenantId, Status);
