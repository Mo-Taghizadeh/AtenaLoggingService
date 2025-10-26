namespace AtenaLoggingService.Abstractions;

public sealed class ColumnMap
{
    public string SchemaName { get; set; } = "Log";
    public string TableName  { get; set; } = "Event";

    public string EventId { get; set; } = "EventId";
    public string EventType { get; set; } = "EventType";        // 1=Request,2=Response,3=Exception,4=Custom
    public string CorrelationId { get; set; } = "CorrelationId";
    public string OccurredAtUtc { get; set; } = "OccurredAtUtc";
    public string ServiceId { get; set; } = "ServiceId";
    public string ServiceMethodId { get; set; } = "ServiceMethodId";
    public string DurationMs { get; set; } = "DurationMs";
    public string Outcome { get; set; } = "Outcome";            // 0=Fail,1=Success
    public string StatusCode { get; set; } = "StatusCode";
    public string DataJson { get; set; } = "DataJson";
    public string InsertTimeUtc { get; set; } = "InsertTimeUtc";
}