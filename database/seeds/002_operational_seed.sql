UPDATE dbo.Yards
SET InboundQueue = 6,
    AverageTurnMinutes = 38
WHERE YardName = 'Seattle North';

UPDATE dbo.Yards
SET InboundQueue = 3,
    AverageTurnMinutes = 46
WHERE YardName = 'Portland East';

INSERT INTO dbo.Yards (TenantId, YardName, Capacity, OccupiedSlots, InboundQueue, AverageTurnMinutes)
VALUES ('tenant-demo', 'Oakland Gateway', 90, 65, 8, 49);

INSERT INTO dbo.Drivers (TenantId, FullName, LicenseNumber, AvailabilityStatus)
VALUES
    ('tenant-demo', 'Ava Patel', 'DL-SEA-1004', 'Assigned'),
    ('tenant-demo', 'Marcus Gray', 'DL-PDX-4411', 'Assigned'),
    ('tenant-demo', 'Nina Chen', 'DL-SEA-8821', 'Standby'),
    ('tenant-demo', 'Ethan Ross', 'DL-PDX-1990', 'Delayed'),
    ('tenant-demo', 'Lena Brooks', 'DL-OAK-5518', 'Assigned');

INSERT INTO dbo.Routes (TenantId, RouteCode, OriginYardId, DestinationName, Status, NextDepartureUtc, CompletionPercent)
VALUES
    ('tenant-demo', 'NW-14', 1, 'Spokane Hub', 'On Schedule', DATEADD(MINUTE, 35, SYSUTCDATETIME()), 68),
    ('tenant-demo', 'OR-03', 2, 'Boise Crossdock', 'Delayed', DATEADD(MINUTE, 75, SYSUTCDATETIME()), 42),
    ('tenant-demo', 'CA-11', 3, 'Reno Freight Park', 'At Risk', DATEADD(MINUTE, 130, SYSUTCDATETIME()), 57),
    ('tenant-demo', 'OR-07', 2, 'Salem Distribution', 'On Schedule', DATEADD(MINUTE, 55, SYSUTCDATETIME()), 74),
    ('tenant-demo', 'PN-22', 1, 'Tacoma Intermodal', 'Awaiting Dispatch', DATEADD(MINUTE, 90, SYSUTCDATETIME()), 62);

UPDATE dbo.Vehicles
SET DriverId = 1,
    RouteId = 1,
    EtaUtc = DATEADD(HOUR, 2, SYSUTCDATETIME()),
    UtilizationPercent = 89
WHERE VehicleId = 'VH-1042';

UPDATE dbo.Vehicles
SET DriverId = 2,
    RouteId = 4,
    EtaUtc = NULL,
    UtilizationPercent = 74
WHERE VehicleId = 'VH-1188';

UPDATE dbo.Vehicles
SET DriverId = 3,
    RouteId = 5,
    EtaUtc = DATEADD(HOUR, 3, SYSUTCDATETIME()),
    UtilizationPercent = 62
WHERE VehicleId = 'VH-1211';

UPDATE dbo.Vehicles
SET DriverId = 4,
    RouteId = 2,
    EtaUtc = DATEADD(HOUR, 4, SYSUTCDATETIME()),
    UtilizationPercent = 51
WHERE VehicleId = 'VH-1305';

INSERT INTO dbo.Vehicles (VehicleId, TenantId, DriverName, Status, CurrentYardId, LastUpdatedUtc, EtaUtc, UtilizationPercent, RouteId, DriverId)
VALUES
    ('VH-1417', 'tenant-demo', 'Lena Brooks', 'In Transit', 3, DATEADD(MINUTE, -2, SYSUTCDATETIME()), DATEADD(HOUR, 1, SYSUTCDATETIME()), 93, 3, 5);

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

INSERT INTO dbo.AlertEvents (TenantId, Severity, Title, Description, OwnerTeam)
VALUES
    ('tenant-demo', 'Critical', 'Trailer queue exceeding target', 'Oakland Gateway has remained above 7 inbound trailers for 35 minutes.', 'Yard Ops'),
    ('tenant-demo', 'Warning', 'Dispatch SLA drift', 'Northwest outbound wave is 9 minutes behind target dispatch time.', 'Routing'),
    ('tenant-demo', 'Info', 'Fleet sync completed', 'Global telematics reconciliation completed without dropped assets.', 'Platform');

INSERT INTO dbo.ReportSnapshots (TenantId, ReportName, MetricValue, ChangeLabel)
VALUES
    ('tenant-demo', 'On-time departure', '96.4%', '+1.2% week over week'),
    ('tenant-demo', 'Average dock turn', '44 min', '-3 min versus last week'),
    ('tenant-demo', 'Asset utilization', '81%', '+4 points this month');

INSERT INTO dbo.UserRoles (TenantId, RoleName)
VALUES
    ('tenant-demo', 'Tenant Admin'),
    ('tenant-demo', 'Dispatcher'),
    ('tenant-demo', 'Yard Manager');

INSERT INTO dbo.Users (TenantId, UserRoleId, DisplayName, EmailAddress)
VALUES
    ('tenant-demo', 1, 'Morgan Ellis', 'morgan.ellis@atlasmeridian.example'),
    ('tenant-demo', 2, 'Priya Shah', 'priya.shah@atlasmeridian.example'),
    ('tenant-demo', 3, 'Darius Cole', 'darius.cole@atlasmeridian.example');
