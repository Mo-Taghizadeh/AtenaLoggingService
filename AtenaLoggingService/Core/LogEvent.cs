using System;
using System.Threading.Channels;

namespace AtenaLoggingService.Core;

public interface IAtenaLogger
{
    void Request(Guid correlationId, int? serviceId, int? methodId, object? data, DateTime? atUtc = null);
    void Response(Guid correlationId, int? serviceId, int? methodId, int? statusCode, int? durationMs, bool? success, object? data, DateTime? atUtc = null);
    void Exception(Guid correlationId, int? serviceId, int? methodId, Exception ex, object? data = null, DateTime? atUtc = null);
}

internal sealed class AtenaLogger : IAtenaLogger
{
    private readonly Channel<LogItem> _channel;

    public AtenaLogger(Channel<LogItem> channel)
    {
        _channel = channel;
    }

    public void Request(Guid correlationId, int? serviceId, int? methodId, object? data, DateTime? atUtc = null)
        => Enqueue(EventType.Request, correlationId, serviceId, methodId, null, null, null, data, atUtc);

    public void Response(Guid correlationId, int? serviceId, int? methodId, int? statusCode, int? durationMs, bool? success, object? data, DateTime? atUtc = null)
        => Enqueue(EventType.Response, correlationId, serviceId, methodId, durationMs, success is null ? null : (byte?)(success.Value ? 1 : 0), statusCode, data, atUtc);

    public void Exception(Guid correlationId, int? serviceId, int? methodId, Exception ex, object? data = null, DateTime? atUtc = null)
        => Enqueue(EventType.Exception, correlationId, serviceId, methodId, null, 0, null, new { error = ex.GetType().FullName, message = ex.Message, data }, atUtc);

    private void Enqueue(EventType type, Guid correlationId, int? svcId, int? methId, int? durationMs, byte? outcome, int? statusCode, object? data, DateTime? atUtc)
    {
        var item = new LogItem
        {
            EventType = type,
            CorrelationId = correlationId,
            OccurredAtUtc = atUtc ?? DateTime.UtcNow,
            ServiceId = svcId,
            ServiceMethodId = methId,
            DurationMs = durationMs,
            Outcome = outcome,
            StatusCode = statusCode,
            DataJson = SafeJson.Serialize(data)
        };
        _channel.Writer.TryWrite(item);
    }
}