using Microsoft.Data.SqlClient;
using AtenaLoggingService.Abstractions;

namespace AtenaLoggingService.Core;

internal static class ConnectionStringFactory
{
    public static string Build(DbInfo d)
    {
        var sb = new SqlConnectionStringBuilder
        {
            DataSource = d.Server,
            InitialCatalog = d.Database,
            IntegratedSecurity = d.IntegratedSecurity,
            Encrypt = d.Encrypt,
            TrustServerCertificate = d.TrustServerCertificate,
            ApplicationName = d.ApplicationName,
            ConnectTimeout = d.ConnectTimeoutSec,
            MultipleActiveResultSets = false
        };
        if (!d.IntegratedSecurity)
        {
            sb.UserID = d.UserId ?? string.Empty;
            sb.Password = d.Password ?? string.Empty;
        }
        return sb.ToString();
    }
}