namespace AtenaLoggingService.Abstractions;

public sealed record JsonProjection(string ColumnName, string JsonPath, bool Persisted = true, bool AddIndex = true);