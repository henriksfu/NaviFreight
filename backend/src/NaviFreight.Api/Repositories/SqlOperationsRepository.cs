using System.Data;
using Microsoft.Data.SqlClient;
using NaviFreight.Api.Contracts;
using NaviFreight.Api.Data;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Repositories;

public sealed class SqlOperationsRepository(ISqlConnectionFactory connectionFactory) : IOperationsRepository
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_DashboardSummary", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new DashboardSummaryResponse(
            reader.GetString(reader.GetOrdinal("TenantId")),
            reader.GetInt32(reader.GetOrdinal("ActiveVehicles")),
            reader.GetInt32(reader.GetOrdinal("YardOccupancyPercent")),
            reader.GetInt32(reader.GetOrdinal("DelayedLoads")),
            reader.GetDecimal(reader.GetOrdinal("OnTimeDispatchRate")),
            reader.GetInt32(reader.GetOrdinal("ActiveRoutes")),
            reader.GetInt32(reader.GetOrdinal("TrailerTurnaroundMinutes")));
    }

    public async Task<IReadOnlyList<FleetVehicleResponse>> GetFleetAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_FleetVehicles", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<FleetVehicleResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new FleetVehicleResponse(
                reader.GetString(reader.GetOrdinal("VehicleId")),
                reader.GetString(reader.GetOrdinal("DriverName")),
                reader.GetString(reader.GetOrdinal("Status")),
                reader.GetString(reader.GetOrdinal("CurrentYard")),
                reader.GetDateTime(reader.GetOrdinal("LastUpdatedUtc")),
                reader.IsDBNull(reader.GetOrdinal("EtaUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("EtaUtc")),
                reader.GetInt32(reader.GetOrdinal("UtilizationPercent")),
                reader.IsDBNull(reader.GetOrdinal("RouteCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("RouteCode"))));
        }

        return items;
    }

    public async Task<FleetVehicleDetailResponse?> GetVehicleByIdAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleDetail", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapVehicleDetail(reader);
    }

    public async Task<FleetVehicleDetailResponse> CreateVehicleAsync(string tenantId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleCreate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = request.VehicleId });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });
        command.Parameters.Add(new SqlParameter("@CurrentYardId", SqlDbType.Int) { Value = request.CurrentYardId is null ? DBNull.Value : request.CurrentYardId });
        command.Parameters.Add(new SqlParameter("@EtaUtc", SqlDbType.DateTime2) { Value = request.EtaUtc is null ? DBNull.Value : request.EtaUtc });
        command.Parameters.Add(new SqlParameter("@UtilizationPercent", SqlDbType.Int) { Value = request.UtilizationPercent });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapVehicleDetail(reader);
    }

    public async Task<FleetVehicleDetailResponse?> UpdateVehicleAsync(string tenantId, string vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });
        command.Parameters.Add(new SqlParameter("@CurrentYardId", SqlDbType.Int) { Value = request.CurrentYardId is null ? DBNull.Value : request.CurrentYardId });
        command.Parameters.Add(new SqlParameter("@EtaUtc", SqlDbType.DateTime2) { Value = request.EtaUtc is null ? DBNull.Value : request.EtaUtc });
        command.Parameters.Add(new SqlParameter("@UtilizationPercent", SqlDbType.Int) { Value = request.UtilizationPercent });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapVehicleDetail(reader);
    }

    public async Task<bool> DeleteVehicleAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleDelete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });

        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        return result > 0;
    }

    public async Task<FleetVehicleDetailResponse?> AssignDriverAsync(string tenantId, string vehicleId, int driverId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleAssignDriver", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });
        command.Parameters.Add(new SqlParameter("@DriverId", SqlDbType.Int) { Value = driverId });
        await command.ExecuteNonQueryAsync(cancellationToken);

        return await GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);
    }

    public async Task<FleetVehicleDetailResponse?> UnassignDriverAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_FleetVehicleUnassignDriver", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });
        await command.ExecuteNonQueryAsync(cancellationToken);

        return await GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);
    }

    public async Task<IReadOnlyList<DriverResponse>> GetDriversAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_Drivers", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<DriverResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapDriver(reader));
        }

        return items;
    }

    public async Task<DriverResponse?> GetDriverByIdAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DriverDetail", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@DriverId", SqlDbType.Int) { Value = driverId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapDriver(reader);
    }

    public async Task<DriverResponse> CreateDriverAsync(string tenantId, CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DriverCreate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 150) { Value = request.FullName });
        command.Parameters.Add(new SqlParameter("@LicenseNumber", SqlDbType.NVarChar, 50) { Value = request.LicenseNumber });
        command.Parameters.Add(new SqlParameter("@AvailabilityStatus", SqlDbType.NVarChar, 50) { Value = request.AvailabilityStatus });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapDriver(reader);
    }

    public async Task<DriverResponse?> UpdateDriverAsync(string tenantId, int driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DriverUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@DriverId", SqlDbType.Int) { Value = driverId });
        command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 150) { Value = request.FullName });
        command.Parameters.Add(new SqlParameter("@LicenseNumber", SqlDbType.NVarChar, 50) { Value = request.LicenseNumber });
        command.Parameters.Add(new SqlParameter("@AvailabilityStatus", SqlDbType.NVarChar, 50) { Value = request.AvailabilityStatus });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapDriver(reader);
    }

    public async Task<bool> DeleteDriverAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DriverDelete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@DriverId", SqlDbType.Int) { Value = driverId });

        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        return result > 0;
    }

    public async Task<IReadOnlyList<YardSnapshotResponse>> GetYardsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_YardSnapshots", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<YardSnapshotResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new YardSnapshotResponse(
                reader.GetString(reader.GetOrdinal("YardName")),
                reader.GetInt32(reader.GetOrdinal("OccupiedSlots")),
                reader.GetInt32(reader.GetOrdinal("TotalSlots")),
                reader.GetInt32(reader.GetOrdinal("InboundQueue")),
                reader.GetInt32(reader.GetOrdinal("AvailableDocks")),
                reader.GetInt32(reader.GetOrdinal("AverageTurnMinutes"))));
        }

        return items;
    }

    public async Task<IReadOnlyList<RouteAssignmentResponse>> GetRoutesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_RouteAssignments", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<RouteAssignmentResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new RouteAssignmentResponse(
                reader.GetString(reader.GetOrdinal("RouteCode")),
                reader.GetString(reader.GetOrdinal("Origin")),
                reader.GetString(reader.GetOrdinal("Destination")),
                reader.GetString(reader.GetOrdinal("Status")),
                reader.GetInt32(reader.GetOrdinal("AssignedVehicles")),
                reader.GetDateTime(reader.GetOrdinal("NextDepartureUtc")),
                reader.GetInt32(reader.GetOrdinal("CompletionPercent"))));
        }

        return items;
    }

    public async Task<RouteDetailResponse?> GetRouteByCodeAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_RouteAssignmentDetail", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = routeCode });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var route = new RouteAssignmentResponse(
            reader.GetString(reader.GetOrdinal("RouteCode")),
            reader.GetString(reader.GetOrdinal("Origin")),
            reader.GetString(reader.GetOrdinal("Destination")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.GetInt32(reader.GetOrdinal("AssignedVehicles")),
            reader.GetDateTime(reader.GetOrdinal("NextDepartureUtc")),
            reader.GetInt32(reader.GetOrdinal("CompletionPercent")));

        var assignedVehicles = new List<RouteDispatchAssignmentResponse>();
        do
        {
            if (reader.IsDBNull(reader.GetOrdinal("VehicleId")))
            {
                continue;
            }

            assignedVehicles.Add(new RouteDispatchAssignmentResponse(
                reader.GetString(reader.GetOrdinal("VehicleId")),
                reader.GetString(reader.GetOrdinal("DriverName")),
                reader.GetString(reader.GetOrdinal("VehicleStatus")),
                reader.GetString(reader.GetOrdinal("CurrentYard")),
                reader.IsDBNull(reader.GetOrdinal("EtaUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("EtaUtc"))));
        }
        while (await reader.ReadAsync(cancellationToken));

        return new RouteDetailResponse(route, assignedVehicles);
    }

    public async Task<RouteAssignmentResponse> CreateRouteAsync(string tenantId, CreateRouteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_RouteCreate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = request.RouteCode });
        command.Parameters.Add(new SqlParameter("@OriginYardId", SqlDbType.Int) { Value = request.OriginYardId });
        command.Parameters.Add(new SqlParameter("@DestinationName", SqlDbType.NVarChar, 150) { Value = request.DestinationName });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });
        command.Parameters.Add(new SqlParameter("@NextDepartureUtc", SqlDbType.DateTime2) { Value = request.NextDepartureUtc });
        command.Parameters.Add(new SqlParameter("@CompletionPercent", SqlDbType.Int) { Value = request.CompletionPercent });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapRouteAssignment(reader);
    }

    public async Task<RouteAssignmentResponse?> UpdateRouteAsync(string tenantId, string routeCode, UpdateRouteRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_RouteUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = routeCode });
        command.Parameters.Add(new SqlParameter("@DestinationName", SqlDbType.NVarChar, 150) { Value = request.DestinationName });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });
        command.Parameters.Add(new SqlParameter("@NextDepartureUtc", SqlDbType.DateTime2) { Value = request.NextDepartureUtc });
        command.Parameters.Add(new SqlParameter("@CompletionPercent", SqlDbType.Int) { Value = request.CompletionPercent });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapRouteAssignment(reader);
    }

    public async Task<bool> DeleteRouteAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_RouteDelete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = routeCode });

        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        return result > 0;
    }

    public async Task<RouteDetailResponse?> AssignVehiclesAsync(string tenantId, string routeCode, IReadOnlyList<string> vehicleIds, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        foreach (var vehicleId in vehicleIds)
        {
            await using var command = new SqlCommand("dbo.usp_RouteAssignVehicle", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddTenantId(tenantId);
            command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = routeCode });
            command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return await GetRouteByCodeAsync(tenantId, routeCode, cancellationToken);
    }

    public async Task<RouteDetailResponse?> UnassignVehicleAsync(string tenantId, string routeCode, string vehicleId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_RouteUnassignVehicle", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@RouteCode", SqlDbType.NVarChar, 50) { Value = routeCode });
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });
        await command.ExecuteNonQueryAsync(cancellationToken);

        return await GetRouteByCodeAsync(tenantId, routeCode, cancellationToken);
    }

    public async Task<IReadOnlyList<AlertItemResponse>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_ActiveAlerts", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<AlertItemResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AlertItemResponse(
                reader.GetString(reader.GetOrdinal("Severity")),
                reader.GetString(reader.GetOrdinal("Title")),
                reader.GetString(reader.GetOrdinal("Description")),
                reader.GetString(reader.GetOrdinal("Owner"))));
        }

        return items;
    }

    public async Task<IReadOnlyList<ReportSnapshotResponse>> GetReportsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_ReportSnapshots", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<ReportSnapshotResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ReportSnapshotResponse(
                reader.GetString(reader.GetOrdinal("ReportName")),
                reader.GetString(reader.GetOrdinal("Value")),
                reader.GetString(reader.GetOrdinal("ChangeLabel"))));
        }

        return items;
    }

    public async Task<IReadOnlyList<SettingsSectionResponse>> GetSettingsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_SettingsSections", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            var section = reader.GetString(reader.GetOrdinal("SectionTitle"));
            var item = reader.GetString(reader.GetOrdinal("ItemValue"));

            if (!grouped.TryGetValue(section, out var items))
            {
                items = [];
                grouped[section] = items;
            }

            items.Add(item);
        }

        return grouped.Select(pair => new SettingsSectionResponse(pair.Key, pair.Value)).ToList();
    }

    private static SqlCommand CreateStoredProcedure(SqlConnection connection, string procedureName, string tenantId)
    {
        var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.AddTenantId(tenantId);
        return command;
    }

    private static RouteAssignmentResponse MapRouteAssignment(SqlDataReader reader)
    {
        return new RouteAssignmentResponse(
            reader.GetString(reader.GetOrdinal("RouteCode")),
            reader.GetString(reader.GetOrdinal("Origin")),
            reader.GetString(reader.GetOrdinal("Destination")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.GetInt32(reader.GetOrdinal("AssignedVehicles")),
            reader.GetDateTime(reader.GetOrdinal("NextDepartureUtc")),
            reader.GetInt32(reader.GetOrdinal("CompletionPercent")));
    }

    private static FleetVehicleDetailResponse MapVehicleDetail(SqlDataReader reader)
    {
        return new FleetVehicleDetailResponse(
            reader.GetString(reader.GetOrdinal("VehicleId")),
            reader.IsDBNull(reader.GetOrdinal("DriverId")) ? null : reader.GetInt32(reader.GetOrdinal("DriverId")),
            reader.GetString(reader.GetOrdinal("DriverName")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.IsDBNull(reader.GetOrdinal("CurrentYardId")) ? null : reader.GetInt32(reader.GetOrdinal("CurrentYardId")),
            reader.GetString(reader.GetOrdinal("CurrentYard")),
            reader.GetDateTime(reader.GetOrdinal("LastUpdatedUtc")),
            reader.IsDBNull(reader.GetOrdinal("EtaUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("EtaUtc")),
            reader.GetInt32(reader.GetOrdinal("UtilizationPercent")),
            reader.IsDBNull(reader.GetOrdinal("RouteCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("RouteCode")),
            reader.GetBoolean(reader.GetOrdinal("IsDispatchReady")));
    }

    private static DriverResponse MapDriver(SqlDataReader reader)
    {
        return new DriverResponse(
            reader.GetInt32(reader.GetOrdinal("DriverId")),
            reader.GetString(reader.GetOrdinal("FullName")),
            reader.GetString(reader.GetOrdinal("LicenseNumber")),
            reader.GetString(reader.GetOrdinal("AvailabilityStatus")),
            reader.IsDBNull(reader.GetOrdinal("AssignedVehicleId")) ? null : reader.GetString(reader.GetOrdinal("AssignedVehicleId")));
    }

    // ── Yards ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<YardResponse>> GetYardListAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateStoredProcedure(connection, "dbo.usp_YardList", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<YardResponse>();
        while (await reader.ReadAsync(cancellationToken))
            items.Add(MapYard(reader));

        return items;
    }

    public async Task<YardDetailResponse?> GetYardDetailAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_YardDetail", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var yard = MapYard(reader);

        await reader.NextResultAsync(cancellationToken);
        var docks = new List<DockResponse>();
        while (await reader.ReadAsync(cancellationToken))
            docks.Add(MapDock(reader));

        return new YardDetailResponse(yard, docks);
    }

    public async Task<YardDetailResponse> CreateYardAsync(string tenantId, CreateYardRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_YardCreate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardName", SqlDbType.NVarChar, 150) { Value = request.YardName });
        command.Parameters.Add(new SqlParameter("@Capacity", SqlDbType.Int) { Value = request.Capacity });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        var yard = MapYard(reader);
        return new YardDetailResponse(yard, []);
    }

    public async Task<YardResponse?> UpdateYardAsync(string tenantId, int yardId, UpdateYardRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_YardUpdate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@YardName", SqlDbType.NVarChar, 150) { Value = request.YardName });
        command.Parameters.Add(new SqlParameter("@Capacity", SqlDbType.Int) { Value = request.Capacity });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapYard(reader);
    }

    public async Task<bool> DeleteYardAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_YardDelete", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return false;

        return reader.GetInt32(reader.GetOrdinal("AffectedRows")) > 0;
    }

    public async Task<YardResponse?> UpdateYardOperationalAsync(string tenantId, int yardId, YardOperationalUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_YardOperationalUpdate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@OccupiedSlots", SqlDbType.Int) { Value = request.OccupiedSlots });
        command.Parameters.Add(new SqlParameter("@InboundQueue", SqlDbType.Int) { Value = request.InboundQueue });
        command.Parameters.Add(new SqlParameter("@AverageTurnMinutes", SqlDbType.Int) { Value = request.AverageTurnMinutes });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapYard(reader);
    }

    // ── Docks ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DockResponse>> GetDocksAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockList", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var items = new List<DockResponse>();
        while (await reader.ReadAsync(cancellationToken))
            items.Add(MapDock(reader));

        return items;
    }

    public async Task<DockResponse?> GetDockDetailAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockDetail", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockId", SqlDbType.Int) { Value = dockId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDock(reader);
    }

    public async Task<DockResponse?> CreateDockAsync(string tenantId, int yardId, CreateDockRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockCreate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockCode", SqlDbType.NVarChar, 50) { Value = request.DockCode });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDock(reader);
    }

    public async Task<DockResponse?> UpdateDockAsync(string tenantId, int yardId, int dockId, UpdateDockRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockUpdate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockId", SqlDbType.Int) { Value = dockId });
        command.Parameters.Add(new SqlParameter("@DockCode", SqlDbType.NVarChar, 50) { Value = request.DockCode });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = request.Status });
        command.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = request.Notes is null ? DBNull.Value : request.Notes });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDock(reader);
    }

    public async Task<bool> DeleteDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockDelete", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockId", SqlDbType.Int) { Value = dockId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return false;

        return reader.GetInt32(reader.GetOrdinal("AffectedRows")) > 0;
    }

    public async Task<DockResponse?> AssignVehicleToDockAsync(string tenantId, int yardId, int dockId, string vehicleId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockAssignVehicle", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockId", SqlDbType.Int) { Value = dockId });
        command.Parameters.Add(new SqlParameter("@VehicleId", SqlDbType.NVarChar, 50) { Value = vehicleId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDock(reader);
    }

    public async Task<DockResponse?> ReleaseDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_DockReleaseVehicle", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@YardId", SqlDbType.Int) { Value = yardId });
        command.Parameters.Add(new SqlParameter("@DockId", SqlDbType.Int) { Value = dockId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDock(reader);
    }

    // ── Alerts (full lifecycle) ───────────────────────────────────────────────

    public async Task<IReadOnlyList<AlertResponse>> GetAlertListAsync(string tenantId, string? status = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertList", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = status is null ? DBNull.Value : status });
        command.Parameters.Add(new SqlParameter("@Severity", SqlDbType.NVarChar, 50) { Value = severity is null ? DBNull.Value : severity });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var items = new List<AlertResponse>();
        while (await reader.ReadAsync(cancellationToken))
            items.Add(MapAlert(reader));

        return items;
    }

    public async Task<AlertResponse?> GetAlertDetailAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertDetail", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapAlert(reader);
    }

    public async Task<AlertResponse> CreateAlertAsync(string tenantId, CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertCreate", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@Severity", SqlDbType.NVarChar, 50) { Value = request.Severity });
        command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 150) { Value = request.Title });
        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 400) { Value = request.Description });
        command.Parameters.Add(new SqlParameter("@OwnerTeam", SqlDbType.NVarChar, 100) { Value = request.OwnerTeam });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapAlert(reader);
    }

    public async Task<AlertResponse?> AcknowledgeAlertAsync(string tenantId, int alertId, AcknowledgeAlertRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertAcknowledge", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });
        command.Parameters.Add(new SqlParameter("@AcknowledgedByEmail", SqlDbType.NVarChar, 200) { Value = request.AcknowledgedByEmail });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapAlert(reader);
    }

    public async Task<AlertResponse?> ResolveAlertAsync(string tenantId, int alertId, ResolveAlertRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertResolve", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });
        command.Parameters.Add(new SqlParameter("@ResolvedByEmail", SqlDbType.NVarChar, 200) { Value = request.ResolvedByEmail });
        command.Parameters.Add(new SqlParameter("@ResolutionNotes", SqlDbType.NVarChar, 1000) { Value = request.ResolutionNotes is null ? DBNull.Value : request.ResolutionNotes });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapAlert(reader);
    }

    public async Task<AlertResponse?> ReopenAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertReopen", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapAlert(reader);
    }

    public async Task<AlertResponse?> UpdateAlertOwnerAsync(string tenantId, int alertId, UpdateAlertOwnerRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertUpdateOwner", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });
        command.Parameters.Add(new SqlParameter("@OwnerTeam", SqlDbType.NVarChar, 100) { Value = request.OwnerTeam });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapAlert(reader);
    }

    public async Task<bool> CloseAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_AlertClose", connection) { CommandType = CommandType.StoredProcedure };
        command.AddTenantId(tenantId);
        command.Parameters.Add(new SqlParameter("@AlertEventId", SqlDbType.Int) { Value = alertId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return false;

        return reader.GetInt32(reader.GetOrdinal("AffectedRows")) > 0;
    }

    private static YardResponse MapYard(SqlDataReader reader) =>
        new(
            reader.GetInt32(reader.GetOrdinal("YardId")),
            reader.GetString(reader.GetOrdinal("YardName")),
            reader.GetInt32(reader.GetOrdinal("Capacity")),
            reader.GetInt32(reader.GetOrdinal("OccupiedSlots")),
            reader.GetInt32(reader.GetOrdinal("InboundQueue")),
            reader.GetInt32(reader.GetOrdinal("AverageTurnMinutes")),
            reader.GetInt32(reader.GetOrdinal("TotalDocks")),
            reader.GetInt32(reader.GetOrdinal("AvailableDocks")),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetDateTime(reader.GetOrdinal("CreatedUtc")),
            reader.IsDBNull(reader.GetOrdinal("UpdatedUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedUtc")));

    private static DockResponse MapDock(SqlDataReader reader) =>
        new(
            reader.GetInt32(reader.GetOrdinal("DockId")),
            reader.GetInt32(reader.GetOrdinal("YardId")),
            reader.GetString(reader.GetOrdinal("DockCode")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.IsDBNull(reader.GetOrdinal("OccupyingVehicleId")) ? null : reader.GetString(reader.GetOrdinal("OccupyingVehicleId")),
            reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            reader.GetDateTime(reader.GetOrdinal("CreatedUtc")),
            reader.IsDBNull(reader.GetOrdinal("UpdatedUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedUtc")));

    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "dbo.usp_UserList";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var results = new List<UserResponse>();
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapUser(reader));
        return results;
    }

    public async Task<UserResponse> CreateUserAsync(string tenantId, CreateUserRequest request, string passwordHash, CancellationToken cancellationToken = default)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "dbo.usp_UserCreate";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@Email", request.Email);
        cmd.Parameters.AddWithValue("@DisplayName", request.DisplayName);
        cmd.Parameters.AddWithValue("@RoleName", request.Role);
        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return MapUser(reader);
    }

    public async Task<UserResponse?> UpdateUserAsync(string tenantId, int userId, UpdateUserRequest request, string? newPasswordHash, CancellationToken cancellationToken = default)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "dbo.usp_UserUpdate";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@DisplayName", request.DisplayName);
        cmd.Parameters.AddWithValue("@RoleName", request.Role);
        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@NewPasswordHash", System.Data.SqlDbType.NVarChar, 200)
            { Value = newPasswordHash is null ? DBNull.Value : newPasswordHash });
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return MapUser(reader);
    }

    public async Task<bool> DeactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "dbo.usp_UserDeactivate";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<UserResponse?> ReactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "dbo.usp_UserReactivate";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TenantId", tenantId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return MapUser(reader);
    }

    private static UserResponse MapUser(SqlDataReader reader) =>
        new(
            reader.GetInt32(reader.GetOrdinal("UserId")),
            reader.GetString(reader.GetOrdinal("EmailAddress")),
            reader.GetString(reader.GetOrdinal("DisplayName")),
            reader.GetString(reader.GetOrdinal("RoleName")),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetDateTime(reader.GetOrdinal("CreatedUtc")));

    private static AlertResponse MapAlert(SqlDataReader reader) =>
        new(
            reader.GetInt32(reader.GetOrdinal("AlertEventId")),
            reader.GetString(reader.GetOrdinal("Severity")),
            reader.GetString(reader.GetOrdinal("Title")),
            reader.GetString(reader.GetOrdinal("Description")),
            reader.GetString(reader.GetOrdinal("OwnerTeam")),
            reader.GetString(reader.GetOrdinal("Status")),
            reader.GetBoolean(reader.GetOrdinal("IsActive")),
            reader.GetDateTime(reader.GetOrdinal("CreatedUtc")),
            reader.IsDBNull(reader.GetOrdinal("AcknowledgedByEmail")) ? null : reader.GetString(reader.GetOrdinal("AcknowledgedByEmail")),
            reader.IsDBNull(reader.GetOrdinal("AcknowledgedUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("AcknowledgedUtc")),
            reader.IsDBNull(reader.GetOrdinal("ResolvedByEmail")) ? null : reader.GetString(reader.GetOrdinal("ResolvedByEmail")),
            reader.IsDBNull(reader.GetOrdinal("ResolvedUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("ResolvedUtc")),
            reader.IsDBNull(reader.GetOrdinal("ResolutionNotes")) ? null : reader.GetString(reader.GetOrdinal("ResolutionNotes")),
            reader.IsDBNull(reader.GetOrdinal("UpdatedUtc")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedUtc")));
}
