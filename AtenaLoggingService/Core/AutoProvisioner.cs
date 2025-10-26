using System;
using System.Data;
using Microsoft.Data.SqlClient;
using AtenaLoggingService.Abstractions;

namespace AtenaLoggingService.Core;

internal sealed class AutoProvisioner
{
    private readonly LoggingOptions _options;

    public AutoProvisioner(LoggingOptions options)
    {
        _options = options;
    }

    public void EnsureProvisioned()
    {
        var cs = ConnectionStringFactory.Build(_options.Database);
        if (_options.Database.CreateDatabaseIfNotExists)
        {
            CreateDatabaseIfMissing(_options.Database);
        }
        CreateSchemaAndTable(cs, _options.Columns);
        ApplyIndexes(cs, _options.Columns);
        ApplyProjections(cs, _options.Columns, _options.Projections);
    }

    private static void CreateDatabaseIfMissing(DbInfo db)
    {
        var master = new SqlConnectionStringBuilder
        {
            DataSource = db.Server,
            InitialCatalog = "master",
            IntegratedSecurity = db.IntegratedSecurity,
            Encrypt = db.Encrypt,
            TrustServerCertificate = db.TrustServerCertificate,
            ApplicationName = db.ApplicationName,
            ConnectTimeout = db.ConnectTimeoutSec
        };
        if (!db.IntegratedSecurity) { master.UserID = db.UserId; master.Password = db.Password; }

        using var conn = new SqlConnection(master.ToString());
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"IF DB_ID(N'{Escape(db.Database)}') IS NULL CREATE DATABASE [{Escape(db.Database)}];";
        cmd.ExecuteNonQuery();
    }

    private static void CreateSchemaAndTable(string cs, ColumnMap map)
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 60;

        cmd.CommandText = $@"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'{Escape(map.SchemaName)}') EXEC('CREATE SCHEMA [{Escape(map.SchemaName)}]');

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.name = N'{Escape(map.TableName)}' AND s.name = N'{Escape(map.SchemaName)}'
)
BEGIN
    CREATE TABLE [{Escape(map.SchemaName)}].[{Escape(map.TableName)}](
        [{Escape(map.EventId)}]        BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [{Escape(map.EventType)}]      TINYINT       NOT NULL,
        [{Escape(map.CorrelationId)}]  UNIQUEIDENTIFIER NOT NULL,
        [{Escape(map.OccurredAtUtc)}]  DATETIME2(3)  NOT NULL,
        [{Escape(map.ServiceId)}]      INT           NULL,
        [{Escape(map.ServiceMethodId)}] INT          NULL,
        [{Escape(map.DurationMs)}]     INT           NULL,
        [{Escape(map.Outcome)}]        TINYINT       NULL,
        [{Escape(map.StatusCode)}]     INT           NULL,
        [{Escape(map.DataJson)}]       NVARCHAR(MAX) NOT NULL,
        [{Escape(map.InsertTimeUtc)}]  DATETIME2(3)  NOT NULL CONSTRAINT DF_{Guid.NewGuid():N} DEFAULT (SYSUTCDATETIME())
    );
END
";
        cmd.ExecuteNonQuery();
    }

    private static void ApplyIndexes(string cs, ColumnMap map)
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 60;
        cmd.CommandText = $@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_{Escape(map.TableName)}_Time_SvcMeth' AND object_id = OBJECT_ID(N'[{Escape(map.SchemaName)}].[{Escape(map.TableName)}]'))
    CREATE INDEX [IX_{Escape(map.TableName)}_Time_SvcMeth] ON [{Escape(map.SchemaName)}].[{Escape(map.TableName)}]([{Escape(map.OccurredAtUtc)}], [{Escape(map.ServiceId)}], [{Escape(map.ServiceMethodId)}]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_{Escape(map.TableName)}_Correlation' AND object_id = OBJECT_ID(N'[{Escape(map.SchemaName)}].[{Escape(map.TableName)}]'))
    CREATE INDEX [IX_{Escape(map.TableName)}_Correlation] ON [{Escape(map.SchemaName)}].[{Escape(map.TableName)}]([{Escape(map.CorrelationId)}]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_{Escape(map.TableName)}_Outcome' AND object_id = OBJECT_ID(N'[{Escape(map.SchemaName)}].[{Escape(map.TableName)}]'))
    CREATE INDEX [IX_{Escape(map.TableName)}_Outcome] ON [{Escape(map.SchemaName)}].[{Escape(map.TableName)}]([{Escape(map.Outcome)}]) INCLUDE ([{Escape(map.DurationMs)}]);
";
        cmd.ExecuteNonQuery();
    }

    private static void ApplyProjections(string cs, ColumnMap map, System.Collections.Generic.IEnumerable<JsonProjection> projections)
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
        foreach (var p in projections)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 60;

            var col = Escape(p.ColumnName);
            var schema = Escape(map.SchemaName);
            var table = Escape(map.TableName);
            var dataCol = Escape(map.DataJson);
            var persisted = p.Persisted ? "PERSISTED" : string.Empty;

            cmd.CommandText = $@"
IF COL_LENGTH('[{schema}].[{table}]', '{col}') IS NULL
BEGIN
    ALTER TABLE [{schema}].[{table}] ADD [{col}] AS JSON_VALUE([{dataCol}], '{p.JsonPath.Replace("'", "''")}') {persisted};
END
";
            cmd.ExecuteNonQuery();

            if (p.AddIndex)
            {
                using var cmdIx = conn.CreateCommand();
                cmdIx.CommandType = CommandType.Text;
                cmdIx.CommandTimeout = 60;
                cmdIx.CommandText = $@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_{table}_{col}' AND object_id = OBJECT_ID(N'[{schema}].[{table}]'))
    CREATE INDEX [IX_{table}_{col}] ON [{schema}].[{table}]([{col}]);
";
                cmdIx.ExecuteNonQuery();
            }
        }
    }

    private static string Escape(string input) => input.Replace("]", "]]");
}