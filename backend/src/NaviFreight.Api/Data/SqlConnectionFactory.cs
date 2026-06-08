using Microsoft.Data.SqlClient;

namespace NaviFreight.Api.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("NaviFreight")
        ?? throw new InvalidOperationException("ConnectionStrings:NaviFreight is not configured.");

    public SqlConnection CreateConnection() => new(_connectionString);
}
