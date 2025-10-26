using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AtenaLoggingService.Abstractions;

namespace AtenaLoggingService.Core;

internal sealed class SqlEventWriter
{
    private readonly LoggingOptions _options;
    private readonly string _cs;

    public SqlEventWriter(LoggingOptions options)
    {
        _options = options;
        _cs = ConnectionStringFactory.Build(options.Database);
    }

    public void WriteBatch(IReadOnlyList<LogItem> batch)
    {
        if (batch.Count == 0) return;

        using var conn = new SqlConnection(_cs);
        conn.Open();

        using var table = new DataTable();
        var m = _options.Columns;
        table.Columns.Add(m.EventType, typeof(byte));
        table.Columns.Add(m.CorrelationId, typeof(Guid));
        table.Columns.Add(m.OccurredAtUtc, typeof(DateTime));
        table.Columns.Add(m.ServiceId, typeof(int));
        table.Columns.Add(m.ServiceMethodId, typeof(int));
        table.Columns.Add(m.DurationMs, typeof(int));
        table.Columns.Add(m.Outcome, typeof(byte));
        table.Columns.Add(m.StatusCode, typeof(int));
        table.Columns.Add(m.DataJson, typeof(string));
        table.Columns.Add(m.InsertTimeUtc, typeof(DateTime));

        foreach (var li in batch)
        {
            table.Rows.Add(
                (byte)li.EventType,
                li.CorrelationId,
                li.OccurredAtUtc,
                (object?)li.ServiceId ?? DBNull.Value,
                (object?)li.ServiceMethodId ?? DBNull.Value,
                (object?)li.DurationMs ?? DBNull.Value,
                (object?)li.Outcome ?? DBNull.Value,
                (object?)li.StatusCode ?? DBNull.Value,
                li.DataJson,
                DateTime.UtcNow
            );
        }

        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = $"[{m.SchemaName}].[{m.TableName}]",
            BulkCopyTimeout = 120
        };

        foreach (DataColumn c in table.Columns)
            bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);

        bulk.WriteToServer(table);
    }
}