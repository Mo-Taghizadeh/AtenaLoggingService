using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AtenaLoggingService.Abstractions;
using AtenaLoggingService.Middleware;

namespace AtenaLoggingService.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAtenaLogging(this IApplicationBuilder app)
    {
        var opts = app.ApplicationServices.GetRequiredService<IOptions<LoggingOptions>>().Value;
        if (opts.EnableExceptionMiddleware)
            app.UseMiddleware<AtenaExceptionMiddleware>();

        app.UseMiddleware<AtenaLoggingMiddleware>();
        return app;
    }
}