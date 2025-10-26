using System.Collections.Generic;

namespace AtenaLoggingService.Abstractions;

public sealed class LoggingOptions
{
    public DbInfo Database { get; set; } = new();
    public ColumnMap Columns { get; set; } = new();

    public List<JsonProjection> Projections { get; } = new();

    // Writer/Buffer
    public int ChannelCapacity { get; set; } = 50_000;
    public int FlushIntervalMs { get; set; } = 1000;
    public int BulkSize { get; set; } = 1000;

    // Middleware flags
    public bool CaptureRequestBodyPreview { get; set; } = false; // optional, to avoid overhead by default
    public bool CaptureResponseBodyPreview { get; set; } = false;

    // General
    public bool EnableExceptionMiddleware { get; set; } = true;
}