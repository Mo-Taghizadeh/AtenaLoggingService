using System;

namespace AtenaLoggingService.Core;

public sealed class LogItem
{
    public EventType EventType { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public int? ServiceId { get; init; }
    public int? ServiceMethodId { get; init; }
    public int? DurationMs { get; init; }
    public byte? Outcome { get; init; } // 0=Fail,1=Success
    public int? StatusCode { get; init; }
    public string DataJson { get; init; } = "{}";
}