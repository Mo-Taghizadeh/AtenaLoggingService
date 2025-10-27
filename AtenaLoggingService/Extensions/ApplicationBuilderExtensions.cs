using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AtenaLoggingService.Abstractions;
using AtenaLoggingService.Middleware;
using AtenaLoggingService.Core;

namespace AtenaLoggingService.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAtenaLogging(this IApplicationBuilder app)
    {
        // 🔧 این دو خط را اضافه کن تا Provision همیشه اجرا شود
        var prov = app.ApplicationServices.GetRequiredService<AutoProvisioner>();
        prov.EnsureProvisioned();

        var opts = app.ApplicationServices.GetRequiredService<IOptions<LoggingOptions>>().Value;
        if (opts.EnableExceptionMiddleware)
            app.UseMiddleware<AtenaExceptionMiddleware>();

        app.UseMiddleware<AtenaLoggingMiddleware>();
        return app;
    }
}