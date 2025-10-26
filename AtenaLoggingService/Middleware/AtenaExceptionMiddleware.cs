using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AtenaLoggingService.Core;

namespace AtenaLoggingService.Middleware;

public sealed class AtenaExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAtenaLogger _logger;

    public AtenaExceptionMiddleware(RequestDelegate next, IAtenaLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            var correlationId = ctx.Response.Headers.ContainsKey("X-Correlation-ID") && Guid.TryParse(ctx.Response.Headers["X-Correlation-ID"], out var cid)
                ? cid
                : Guid.NewGuid();

            _logger.Exception(correlationId, null, null, ex);
            throw;
        }
    }
}