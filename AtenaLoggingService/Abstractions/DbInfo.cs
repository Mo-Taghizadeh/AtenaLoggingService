namespace AtenaLoggingService.Abstractions;

public sealed class DbInfo
{
    public string Server { get; set; } = "localhost";  // e.g. "localhost,1433"
    public string Database { get; set; } = "AtenaLogs";
    public bool IntegratedSecurity { get; set; } = false;
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public bool Encrypt { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = true;
    public int ConnectTimeoutSec { get; set; } = 15;
    public string ApplicationName { get; set; } = "AtenaLoggingService";
    public bool CreateDatabaseIfNotExists { get; set; } = true;
}