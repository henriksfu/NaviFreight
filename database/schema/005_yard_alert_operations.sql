-- ── Yards: add soft-delete flag and update timestamp ─────────────────────────
ALTER TABLE dbo.Yards
ADD IsActive    BIT       NOT NULL CONSTRAINT DF_Yards_IsActive DEFAULT 1,
    UpdatedUtc  DATETIME2 NULL;

-- ── Docks: add vehicle occupancy, notes, and update timestamp ─────────────────
ALTER TABLE dbo.Docks
ADD OccupyingVehicleId NVARCHAR(50) NULL,
    Notes              NVARCHAR(500) NULL,
    UpdatedUtc         DATETIME2    NULL,
    CONSTRAINT FK_Docks_Vehicles
        FOREIGN KEY (OccupyingVehicleId) REFERENCES dbo.Vehicles(VehicleId);

-- ── AlertEvents: full lifecycle columns ───────────────────────────────────────
-- Status values: Active | Acknowledged | Resolved | Closed
ALTER TABLE dbo.AlertEvents
ADD Status              NVARCHAR(20)  NOT NULL CONSTRAINT DF_AlertEvents_Status DEFAULT 'Active',
    AcknowledgedByEmail NVARCHAR(200) NULL,
    AcknowledgedUtc     DATETIME2     NULL,
    ResolvedByEmail     NVARCHAR(200) NULL,
    ResolvedUtc         DATETIME2     NULL,
    ResolutionNotes     NVARCHAR(1000) NULL,
    UpdatedUtc          DATETIME2     NULL;

-- Backfill status for existing rows (IsActive drives Status until this migration)
UPDATE dbo.AlertEvents
SET Status = CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Closed' END;

-- ── Supporting indexes ────────────────────────────────────────────────────────
CREATE INDEX IX_AlertEvents_TenantId_Status
    ON dbo.AlertEvents (TenantId, Status);

CREATE INDEX IX_Docks_YardId_Status
    ON dbo.Docks (YardId, Status);
