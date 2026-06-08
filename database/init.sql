-- NaviFreight master database initialisation
-- Run this once against a fresh SQL Server instance.
-- Idempotent: checks for object existence before creating.

-- ─────────────────────────────────────────────────────────────────────────────
-- 0. Create database
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'NaviFreight')
BEGIN
    CREATE DATABASE NaviFreight;
    PRINT 'Created database NaviFreight.';
END
GO

USE NaviFreight;
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. Base schema (001)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Tenants' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Tenants (
        TenantId  NVARCHAR(50)  NOT NULL PRIMARY KEY,
        Name      NVARCHAR(200) NOT NULL,
        IsActive  BIT           NOT NULL DEFAULT 1,
        CreatedUtc DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME()
    );
    PRINT 'Created dbo.Tenants.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Yards' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Yards (
        YardId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId    NVARCHAR(50)  NOT NULL,
        YardName    NVARCHAR(150) NOT NULL,
        Capacity    INT           NOT NULL,
        OccupiedSlots INT         NOT NULL DEFAULT 0,
        CreatedUtc  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Yards_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
    );
    CREATE INDEX IX_Yards_TenantId ON dbo.Yards (TenantId);
    PRINT 'Created dbo.Yards.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vehicles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Vehicles (
        VehicleId     NVARCHAR(50)  NOT NULL PRIMARY KEY,
        TenantId      NVARCHAR(50)  NOT NULL,
        DriverName    NVARCHAR(150) NOT NULL,
        Status        NVARCHAR(50)  NOT NULL,
        CurrentYardId INT           NULL,
        LastUpdatedUtc DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Vehicles_Tenants FOREIGN KEY (TenantId)      REFERENCES dbo.Tenants(TenantId),
        CONSTRAINT FK_Vehicles_Yards   FOREIGN KEY (CurrentYardId) REFERENCES dbo.Yards(YardId)
    );
    CREATE INDEX IX_Vehicles_TenantId_Status ON dbo.Vehicles (TenantId, Status);
    PRINT 'Created dbo.Vehicles.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. Operational domain (002)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Yards') AND name = 'InboundQueue')
BEGIN
    ALTER TABLE dbo.Yards
    ADD InboundQueue       INT NOT NULL CONSTRAINT DF_Yards_InboundQueue       DEFAULT 0,
        AverageTurnMinutes INT NOT NULL CONSTRAINT DF_Yards_AverageTurnMinutes DEFAULT 0;
    PRINT 'Added InboundQueue/AverageTurnMinutes to dbo.Yards.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Vehicles') AND name = 'EtaUtc')
BEGIN
    ALTER TABLE dbo.Vehicles
    ADD EtaUtc             DATETIME2 NULL,
        UtilizationPercent INT NOT NULL CONSTRAINT DF_Vehicles_UtilizationPercent DEFAULT 0;
    PRINT 'Added EtaUtc/UtilizationPercent to dbo.Vehicles.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Drivers' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Drivers (
        DriverId           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId           NVARCHAR(50)  NOT NULL,
        FullName           NVARCHAR(150) NOT NULL,
        LicenseNumber      NVARCHAR(50)  NOT NULL,
        AvailabilityStatus NVARCHAR(50)  NOT NULL,
        CreatedUtc         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Drivers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
    );
    CREATE INDEX IX_Drivers_TenantId ON dbo.Drivers (TenantId);
    PRINT 'Created dbo.Drivers.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Routes' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Routes (
        RouteId          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId         NVARCHAR(50)  NOT NULL,
        RouteCode        NVARCHAR(50)  NOT NULL,
        OriginYardId     INT           NOT NULL,
        DestinationName  NVARCHAR(150) NOT NULL,
        Status           NVARCHAR(50)  NOT NULL,
        NextDepartureUtc DATETIME2     NOT NULL,
        CompletionPercent INT          NOT NULL DEFAULT 0,
        CreatedUtc       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Routes_Tenants FOREIGN KEY (TenantId)     REFERENCES dbo.Tenants(TenantId),
        CONSTRAINT FK_Routes_Yards   FOREIGN KEY (OriginYardId) REFERENCES dbo.Yards(YardId),
        CONSTRAINT UQ_Routes_TenantId_RouteCode UNIQUE (TenantId, RouteCode)
    );
    CREATE INDEX IX_Routes_TenantId_Status ON dbo.Routes (TenantId, Status);
    PRINT 'Created dbo.Routes.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Vehicles') AND name = 'RouteId')
BEGIN
    ALTER TABLE dbo.Vehicles
    ADD RouteId  INT NULL,
        DriverId INT NULL;
    ALTER TABLE dbo.Vehicles
    ADD CONSTRAINT FK_Vehicles_Routes  FOREIGN KEY (RouteId)  REFERENCES dbo.Routes(RouteId),
        CONSTRAINT FK_Vehicles_Drivers FOREIGN KEY (DriverId) REFERENCES dbo.Drivers(DriverId);
    PRINT 'Added RouteId/DriverId to dbo.Vehicles.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Docks' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Docks (
        DockId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        YardId    INT           NOT NULL,
        DockCode  NVARCHAR(50)  NOT NULL,
        Status    NVARCHAR(50)  NOT NULL,
        CreatedUtc DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Docks_Yards FOREIGN KEY (YardId) REFERENCES dbo.Yards(YardId),
        CONSTRAINT UQ_Docks_YardId_DockCode UNIQUE (YardId, DockCode)
    );
    PRINT 'Created dbo.Docks.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertEvents' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AlertEvents (
        AlertEventId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId     NVARCHAR(50)  NOT NULL,
        Severity     NVARCHAR(50)  NOT NULL,
        Title        NVARCHAR(150) NOT NULL,
        Description  NVARCHAR(400) NOT NULL,
        OwnerTeam    NVARCHAR(100) NOT NULL,
        IsActive     BIT           NOT NULL DEFAULT 1,
        CreatedUtc   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AlertEvents_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
    );
    CREATE INDEX IX_AlertEvents_TenantId_IsActive ON dbo.AlertEvents (TenantId, IsActive);
    PRINT 'Created dbo.AlertEvents.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportSnapshots' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ReportSnapshots (
        ReportSnapshotId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId         NVARCHAR(50)  NOT NULL,
        ReportName       NVARCHAR(100) NOT NULL,
        MetricValue      NVARCHAR(50)  NOT NULL,
        ChangeLabel      NVARCHAR(100) NOT NULL,
        RecordedUtc      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ReportSnapshots_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
    );
    CREATE INDEX IX_ReportSnapshots_TenantId_RecordedUtc ON dbo.ReportSnapshots (TenantId, RecordedUtc DESC);
    PRINT 'Created dbo.ReportSnapshots.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserRoles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserRoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId   NVARCHAR(50)  NOT NULL,
        RoleName   NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_UserRoles_Tenants        FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
        CONSTRAINT UQ_UserRoles_TenantId_RoleName UNIQUE (TenantId, RoleName)
    );
    PRINT 'Created dbo.UserRoles.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Users (
        UserId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId     NVARCHAR(50)  NOT NULL,
        UserRoleId   INT           NOT NULL,
        DisplayName  NVARCHAR(150) NOT NULL,
        EmailAddress NVARCHAR(200) NOT NULL,
        IsActive     BIT           NOT NULL DEFAULT 1,
        CreatedUtc   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Users_Tenants   FOREIGN KEY (TenantId)   REFERENCES dbo.Tenants(TenantId),
        CONSTRAINT FK_Users_UserRoles FOREIGN KEY (UserRoleId) REFERENCES dbo.UserRoles(UserRoleId),
        CONSTRAINT UQ_Users_TenantId_EmailAddress UNIQUE (TenantId, EmailAddress)
    );
    PRINT 'Created dbo.Users.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. Auth schema (003)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE dbo.Users ADD PasswordHash NVARCHAR(200) NULL;
    PRINT 'Added PasswordHash to dbo.Users.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_TenantId_EmailAddress' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    CREATE INDEX IX_Users_TenantId_EmailAddress ON dbo.Users (TenantId, EmailAddress);
    PRINT 'Created IX_Users_TenantId_EmailAddress.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 4. Settings schema (004)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantSettings' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TenantSettings (
        TenantSettingId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId        NVARCHAR(50)  NOT NULL,
        SettingKey      NVARCHAR(100) NOT NULL,
        SettingValue    NVARCHAR(250) NOT NULL,
        CONSTRAINT FK_TenantSettings_Tenants           FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
        CONSTRAINT UQ_TenantSettings_TenantId_SettingKey UNIQUE (TenantId, SettingKey)
    );
    CREATE INDEX IX_TenantSettings_TenantId ON dbo.TenantSettings (TenantId);
    PRINT 'Created dbo.TenantSettings.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DispatchRules' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.DispatchRules (
        DispatchRuleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId       NVARCHAR(50)  NOT NULL,
        RuleName       NVARCHAR(120) NOT NULL,
        RuleValue      NVARCHAR(250) NOT NULL,
        IsEnabled      BIT           NOT NULL DEFAULT 1,
        CONSTRAINT FK_DispatchRules_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
    );
    CREATE INDEX IX_DispatchRules_TenantId ON dbo.DispatchRules (TenantId);
    PRINT 'Created dbo.DispatchRules.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 5. Yard + alert lifecycle columns (005)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Yards') AND name = 'IsActive')
BEGIN
    ALTER TABLE dbo.Yards
    ADD IsActive   BIT       NOT NULL CONSTRAINT DF_Yards_IsActive DEFAULT 1,
        UpdatedUtc DATETIME2 NULL;
    PRINT 'Added IsActive/UpdatedUtc to dbo.Yards.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Docks') AND name = 'OccupyingVehicleId')
BEGIN
    ALTER TABLE dbo.Docks
    ADD OccupyingVehicleId NVARCHAR(50)  NULL,
        Notes              NVARCHAR(500) NULL,
        UpdatedUtc         DATETIME2     NULL,
        CONSTRAINT FK_Docks_Vehicles FOREIGN KEY (OccupyingVehicleId) REFERENCES dbo.Vehicles(VehicleId);
    PRINT 'Added OccupyingVehicleId/Notes/UpdatedUtc to dbo.Docks.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AlertEvents') AND name = 'Status')
BEGIN
    ALTER TABLE dbo.AlertEvents
    ADD Status              NVARCHAR(20)   NOT NULL CONSTRAINT DF_AlertEvents_Status DEFAULT 'Active',
        AcknowledgedByEmail NVARCHAR(200)  NULL,
        AcknowledgedUtc     DATETIME2      NULL,
        ResolvedByEmail     NVARCHAR(200)  NULL,
        ResolvedUtc         DATETIME2      NULL,
        ResolutionNotes     NVARCHAR(1000) NULL,
        UpdatedUtc          DATETIME2      NULL;
    -- Backfill existing rows
    UPDATE dbo.AlertEvents SET Status = CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Closed' END;
    PRINT 'Added lifecycle columns to dbo.AlertEvents.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AlertEvents_TenantId_Status' AND object_id = OBJECT_ID('dbo.AlertEvents'))
BEGIN
    CREATE INDEX IX_AlertEvents_TenantId_Status ON dbo.AlertEvents (TenantId, Status);
    PRINT 'Created IX_AlertEvents_TenantId_Status.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Docks_YardId_Status' AND object_id = OBJECT_ID('dbo.Docks'))
BEGIN
    CREATE INDEX IX_Docks_YardId_Status ON dbo.Docks (YardId, Status);
    PRINT 'Created IX_Docks_YardId_Status.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 6. Demo seed data (skip if tenant already exists)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE TenantId = 'tenant-demo')
BEGIN
    PRINT 'Seeding demo data...';

    INSERT INTO dbo.Tenants (TenantId, Name) VALUES ('tenant-demo', 'Atlas Meridian Logistics');

    -- Yards
    INSERT INTO dbo.Yards (TenantId, YardName, Capacity, OccupiedSlots, InboundQueue, AverageTurnMinutes)
    VALUES
        ('tenant-demo', 'Seattle North',  120, 83, 6, 38),
        ('tenant-demo', 'Portland East',   75, 42, 3, 46),
        ('tenant-demo', 'Oakland Gateway', 90, 65, 8, 49);

    -- Roles
    INSERT INTO dbo.UserRoles (TenantId, RoleName)
    VALUES
        ('tenant-demo', 'Tenant Admin'),
        ('tenant-demo', 'Dispatcher'),
        ('tenant-demo', 'Yard Manager');

    -- Users (passwords set below)
    INSERT INTO dbo.Users (TenantId, UserRoleId, DisplayName, EmailAddress)
    VALUES
        ('tenant-demo', 1, 'Morgan Ellis', 'morgan.ellis@atlasmeridian.example'),
        ('tenant-demo', 2, 'Priya Shah',   'priya.shah@atlasmeridian.example'),
        ('tenant-demo', 3, 'Darius Cole',  'darius.cole@atlasmeridian.example');

    -- Drivers
    INSERT INTO dbo.Drivers (TenantId, FullName, LicenseNumber, AvailabilityStatus)
    VALUES
        ('tenant-demo', 'Ava Patel',    'DL-SEA-1004', 'On Duty'),
        ('tenant-demo', 'Marcus Gray',  'DL-PDX-4411', 'On Duty'),
        ('tenant-demo', 'Nina Chen',    'DL-SEA-8821', 'Available'),
        ('tenant-demo', 'Ethan Ross',   'DL-PDX-1990', 'On Duty'),
        ('tenant-demo', 'Lena Brooks',  'DL-OAK-5518', 'On Duty');

    -- Routes (use fixed future departure offsets)
    INSERT INTO dbo.Routes (TenantId, RouteCode, OriginYardId, DestinationName, Status, NextDepartureUtc, CompletionPercent)
    VALUES
        ('tenant-demo', 'NW-14', 1, 'Spokane Hub',          'On Schedule',       DATEADD(MINUTE,  35, SYSUTCDATETIME()), 68),
        ('tenant-demo', 'OR-03', 2, 'Boise Crossdock',      'Delayed',           DATEADD(MINUTE,  75, SYSUTCDATETIME()), 42),
        ('tenant-demo', 'CA-11', 3, 'Reno Freight Park',    'At Risk',           DATEADD(MINUTE, 130, SYSUTCDATETIME()), 57),
        ('tenant-demo', 'OR-07', 2, 'Salem Distribution',   'On Schedule',       DATEADD(MINUTE,  55, SYSUTCDATETIME()), 74),
        ('tenant-demo', 'PN-22', 1, 'Tacoma Intermodal',    'On Schedule',       DATEADD(MINUTE,  90, SYSUTCDATETIME()), 62);

    -- Vehicles
    INSERT INTO dbo.Vehicles (VehicleId, TenantId, DriverName, Status, CurrentYardId, EtaUtc, UtilizationPercent, RouteId, DriverId)
    VALUES
        ('VH-1042', 'tenant-demo', 'Ava Patel',   'In Transit',        1, DATEADD(HOUR, 2, SYSUTCDATETIME()), 89, 1, 1),
        ('VH-1188', 'tenant-demo', 'Marcus Gray',  'At Dock',           2, NULL,                              74, 4, 2),
        ('VH-1211', 'tenant-demo', 'Nina Chen',    'Awaiting Dispatch', 1, DATEADD(HOUR, 3, SYSUTCDATETIME()), 62, 5, 3),
        ('VH-1305', 'tenant-demo', 'Ethan Ross',   'Delayed',           2, DATEADD(HOUR, 4, SYSUTCDATETIME()), 51, 2, 4),
        ('VH-1417', 'tenant-demo', 'Lena Brooks',  'In Transit',        3, DATEADD(HOUR, 1, SYSUTCDATETIME()), 93, 3, 5);

    -- Docks
    INSERT INTO dbo.Docks (YardId, DockCode, Status)
    VALUES
        (1, 'SEA-D1', 'Occupied'),
        (1, 'SEA-D2', 'Available'),
        (1, 'SEA-D3', 'Available'),
        (2, 'PDX-D1', 'Available'),
        (2, 'PDX-D2', 'Available'),
        (2, 'PDX-D3', 'Available'),
        (2, 'PDX-D4', 'Available'),
        (3, 'OAK-D1', 'Occupied'),
        (3, 'OAK-D2', 'Available');

    -- Alerts
    INSERT INTO dbo.AlertEvents (TenantId, Severity, Title, Description, OwnerTeam, Status)
    VALUES
        ('tenant-demo', 'Critical', 'Trailer queue exceeding target',
         'Oakland Gateway has remained above 7 inbound trailers for 35 minutes.', 'Yard Ops', 'Active'),
        ('tenant-demo', 'Warning', 'Dispatch SLA drift',
         'Northwest outbound wave is 9 minutes behind target dispatch time.', 'Routing', 'Active'),
        ('tenant-demo', 'Info', 'Fleet sync completed',
         'Global telematics reconciliation completed without dropped assets.', 'Platform', 'Active');

    -- Report snapshots
    INSERT INTO dbo.ReportSnapshots (TenantId, ReportName, MetricValue, ChangeLabel)
    VALUES
        ('tenant-demo', 'On-time departure', '96.4%', '+1.2% week over week'),
        ('tenant-demo', 'Average dock turn',  '44 min', '-3 min versus last week'),
        ('tenant-demo', 'Asset utilization',  '81%',   '+4 points this month');

    -- Settings
    INSERT INTO dbo.TenantSettings (TenantId, SettingKey, SettingValue)
    VALUES
        ('tenant-demo', 'Tenant header mapping',       'X-Tenant-Id'),
        ('tenant-demo', 'Default operational region',  'Pacific'),
        ('tenant-demo', 'Brand profile',               'Atlas Meridian Logistics');

    INSERT INTO dbo.DispatchRules (TenantId, RuleName, RuleValue, IsEnabled)
    VALUES
        ('tenant-demo', 'Auto-flag delays after',        '12 minutes',       1),
        ('tenant-demo', 'Recompute dock assignment every', '90 seconds',     1),
        ('tenant-demo', 'Escalate missed departure after', '2 failed retries', 1);

    PRINT 'Demo seed data inserted.';
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 7. Password hashes for demo users
--    SHA-256 (lowercase hex):
--      morgan.ellis  →  demo@Admin1
--      priya.shah    →  demo@Disp1
--      darius.cole   →  demo@Yard1
-- ─────────────────────────────────────────────────────────────────────────────
UPDATE dbo.Users
SET PasswordHash = 'd90bd915b31118219874647d0ebaef274a359fad528dc0942ecd3506b9cc039d'
WHERE EmailAddress = 'morgan.ellis@atlasmeridian.example' AND TenantId = 'tenant-demo'
  AND (PasswordHash IS NULL OR PasswordHash = '');

UPDATE dbo.Users
SET PasswordHash = '22c1fafcc463eb1247aef9914baaa1dae0a985a2979fe63ae8327224f4ba928c'
WHERE EmailAddress = 'priya.shah@atlasmeridian.example' AND TenantId = 'tenant-demo'
  AND (PasswordHash IS NULL OR PasswordHash = '');

UPDATE dbo.Users
SET PasswordHash = '824894a8891f45a5ae560d0292986b2f9ee09f764a4b08dfb6b90e447cded838'
WHERE EmailAddress = 'darius.cole@atlasmeridian.example' AND TenantId = 'tenant-demo'
  AND (PasswordHash IS NULL OR PasswordHash = '');

PRINT 'Password hashes set.';
GO
