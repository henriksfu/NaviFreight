ALTER TABLE dbo.Yards
ADD InboundQueue INT NOT NULL CONSTRAINT DF_Yards_InboundQueue DEFAULT 0,
    AverageTurnMinutes INT NOT NULL CONSTRAINT DF_Yards_AverageTurnMinutes DEFAULT 0;

ALTER TABLE dbo.Vehicles
ADD EtaUtc DATETIME2 NULL,
    UtilizationPercent INT NOT NULL CONSTRAINT DF_Vehicles_UtilizationPercent DEFAULT 0;

CREATE TABLE dbo.Drivers (
    DriverId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    LicenseNumber NVARCHAR(50) NOT NULL,
    AvailabilityStatus NVARCHAR(50) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Drivers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE TABLE dbo.Routes (
    RouteId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    RouteCode NVARCHAR(50) NOT NULL,
    OriginYardId INT NOT NULL,
    DestinationName NVARCHAR(150) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    NextDepartureUtc DATETIME2 NOT NULL,
    CompletionPercent INT NOT NULL DEFAULT 0,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Routes_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT FK_Routes_Yards FOREIGN KEY (OriginYardId) REFERENCES dbo.Yards(YardId),
    CONSTRAINT UQ_Routes_TenantId_RouteCode UNIQUE (TenantId, RouteCode)
);

ALTER TABLE dbo.Vehicles
ADD RouteId INT NULL,
    DriverId INT NULL;

ALTER TABLE dbo.Vehicles
ADD CONSTRAINT FK_Vehicles_Routes FOREIGN KEY (RouteId) REFERENCES dbo.Routes(RouteId),
    CONSTRAINT FK_Vehicles_Drivers FOREIGN KEY (DriverId) REFERENCES dbo.Drivers(DriverId);

CREATE TABLE dbo.Docks (
    DockId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    YardId INT NOT NULL,
    DockCode NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Docks_Yards FOREIGN KEY (YardId) REFERENCES dbo.Yards(YardId),
    CONSTRAINT UQ_Docks_YardId_DockCode UNIQUE (YardId, DockCode)
);

CREATE TABLE dbo.AlertEvents (
    AlertEventId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(400) NOT NULL,
    OwnerTeam NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AlertEvents_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE TABLE dbo.ReportSnapshots (
    ReportSnapshotId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    ReportName NVARCHAR(100) NOT NULL,
    MetricValue NVARCHAR(50) NOT NULL,
    ChangeLabel NVARCHAR(100) NOT NULL,
    RecordedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ReportSnapshots_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE TABLE dbo.UserRoles (
    UserRoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    RoleName NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_UserRoles_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_UserRoles_TenantId_RoleName UNIQUE (TenantId, RoleName)
);

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId NVARCHAR(50) NOT NULL,
    UserRoleId INT NOT NULL,
    DisplayName NVARCHAR(150) NOT NULL,
    EmailAddress NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT FK_Users_UserRoles FOREIGN KEY (UserRoleId) REFERENCES dbo.UserRoles(UserRoleId),
    CONSTRAINT UQ_Users_TenantId_EmailAddress UNIQUE (TenantId, EmailAddress)
);

CREATE INDEX IX_Drivers_TenantId ON dbo.Drivers (TenantId);
CREATE INDEX IX_Routes_TenantId_Status ON dbo.Routes (TenantId, Status);
CREATE INDEX IX_Vehicles_TenantId_RouteId ON dbo.Vehicles (TenantId, RouteId);
CREATE INDEX IX_AlertEvents_TenantId_IsActive ON dbo.AlertEvents (TenantId, IsActive);
CREATE INDEX IX_ReportSnapshots_TenantId_RecordedUtc ON dbo.ReportSnapshots (TenantId, RecordedUtc DESC);
