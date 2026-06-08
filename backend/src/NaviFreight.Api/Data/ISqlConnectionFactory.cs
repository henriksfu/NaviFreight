using Microsoft.Data.SqlClient;

namespace NaviFreight.Api.Data;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
