using Newtonsoft.Json;

namespace AtenaLoggingService.Core;

internal static class SafeJson
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Formatting = Formatting.None
    };

    public static string Serialize(object? obj)
        => obj is null ? "{}" : JsonConvert.SerializeObject(obj, Settings);
}