using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using AtenaLoggingService.Core;

namespace AtenaLoggingService.Middleware;

public sealed class AtenaLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAtenaLogger _logger;

    public AtenaLoggingMiddleware(RequestDelegate next, IAtenaLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        var correlationId = GetOrCreateCorrelationId(ctx);
        int? svcId = null;
        int? methId = null;

        _logger.Request(correlationId, svcId, methId, new
        {
            http = new
            {
                method = ctx.Request.Method,
                path = ctx.Request.Path.Value,
                query = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : null
            },
            client = new { ip = ctx.Connection.RemoteIpAddress?.ToString() }
        });

        var sw = Stopwatch.StartNew();
        int? statusCode = null;
        try
        {
            await _next(ctx);
            statusCode = ctx.Response?.StatusCode;
        }
        finally
        {
            sw.Stop();
            _logger.Response(correlationId, svcId, methId, statusCode, (int)sw.ElapsedMilliseconds, statusCode is >= 200 and < 500, null);
        }
    }

    private static Guid GetOrCreateCorrelationId(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues v) && Guid.TryParse(v.ToString(), out var gid))
            return gid;
        var id = Guid.NewGuid();
        ctx.Response.Headers["X-Correlation-ID"] = id.ToString();
        return id;
    }
}