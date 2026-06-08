# Local Setup

## Prerequisites

- Node.js 22.x or later
- npm 11.x or later
- .NET SDK 8.x
- SQL Server or Azure SQL-compatible instance

## Frontend

```bash
cd frontend
npm install
npm start
```

Default URL: `http://localhost:4200`

## Backend

```bash
cd backend/src/NaviFreight.Api
dotnet restore
dotnet run
```

Default URL: `http://localhost:5000`

## Database

Run the SQL assets in this order:

1. `database/schema/001_base_schema.sql`
2. `database/schema/002_operational_domain.sql`
3. `database/schema/003_auth_schema.sql`
4. `database/schema/004_settings_schema.sql`
5. `database/stored-procedures/usp_DashboardSummary.sql`
6. `database/stored-procedures/usp_FleetVehicles.sql`
7. `database/stored-procedures/usp_FleetVehicleDetail.sql`
8. `database/stored-procedures/usp_FleetVehicleCreate.sql`
9. `database/stored-procedures/usp_FleetVehicleUpdate.sql`
10. `database/stored-procedures/usp_FleetVehicleDelete.sql`
11. `database/stored-procedures/usp_FleetVehicleAssignDriver.sql`
12. `database/stored-procedures/usp_FleetVehicleUnassignDriver.sql`
13. `database/stored-procedures/usp_Drivers.sql`
14. `database/stored-procedures/usp_DriverDetail.sql`
15. `database/stored-procedures/usp_DriverCreate.sql`
16. `database/stored-procedures/usp_DriverUpdate.sql`
17. `database/stored-procedures/usp_DriverDelete.sql`
18. `database/stored-procedures/usp_YardSnapshots.sql`
19. `database/stored-procedures/usp_RouteAssignments.sql`
20. `database/stored-procedures/usp_RouteAssignmentDetail.sql`
21. `database/stored-procedures/usp_RouteCreate.sql`
22. `database/stored-procedures/usp_RouteUpdate.sql`
23. `database/stored-procedures/usp_RouteDelete.sql`
24. `database/stored-procedures/usp_RouteAssignVehicle.sql`
25. `database/stored-procedures/usp_RouteUnassignVehicle.sql`
26. `database/stored-procedures/usp_ActiveAlerts.sql`
27. `database/stored-procedures/usp_ReportSnapshots.sql`
28. `database/stored-procedures/usp_SettingsSections.sql`
29. `database/seeds/001_demo_seed.sql`
30. `database/seeds/002_operational_seed.sql`
31. `database/seeds/003_auth_seed.sql`
32. `database/seeds/004_settings_seed.sql`

## Tenant Header

The backend expects `X-Tenant-Id`. Local default: `tenant-demo`
