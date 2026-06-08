INSERT INTO dbo.Tenants (TenantId, Name)
VALUES ('tenant-demo', 'Demo Logistics');

INSERT INTO dbo.Yards (TenantId, YardName, Capacity, OccupiedSlots)
VALUES
    ('tenant-demo', 'Seattle North', 120, 83),
    ('tenant-demo', 'Portland East', 75, 42);

INSERT INTO dbo.Vehicles (VehicleId, TenantId, DriverName, Status, CurrentYardId)
VALUES
    ('VH-1042', 'tenant-demo', 'Ava Patel', 'In Transit', 1),
    ('VH-1188', 'tenant-demo', 'Marcus Gray', 'At Dock', 2),
    ('VH-1211', 'tenant-demo', 'Nina Chen', 'Awaiting Dispatch', 1),
    ('VH-1305', 'tenant-demo', 'Ethan Ross', 'Delayed', 2);
