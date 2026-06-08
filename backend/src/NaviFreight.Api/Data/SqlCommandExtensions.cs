using System.Data;
using Microsoft.Data.SqlClient;

namespace NaviFreight.Api.Data;

public static class SqlCommandExtensions
{
    public static SqlParameter AddTenantId(this SqlCommand command, string tenantId)
    {
        return command.Parameters.Add(new SqlParameter("@TenantId", SqlDbType.NVarChar, 50)
        {
            Value = tenantId
        });
    }
}
