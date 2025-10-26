using System;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using AtenaLoggingService.Abstractions;
using AtenaLoggingService.Core;

namespace AtenaLoggingService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAtenaLogging(this IServiceCollection services, Action<LoggingOptions>? configure = null)
    {
        services.AddOptions<LoggingOptions>();
        if (configure != null) services.Configure(configure);

        services.TryAddSingleton<IAtenaLogger, AtenaLogger>();
        services.TryAddSingleton(provider =>
        {
            var opts = provider.GetRequiredService<IOptions<LoggingOptions>>().Value;
            return Channel.CreateBounded<LogItem>(opts.ChannelCapacity);
        });

        services.AddSingleton(provider =>
        {
            var opts = provider.GetRequiredService<IOptions<LoggingOptions>>().Value;
            return new SqlEventWriter(opts);
        });

        // Auto-Provision on startup
        services.AddSingleton(provider =>
        {
            var opts = provider.GetRequiredService<IOptions<LoggingOptions>>().Value;
            var prov = new AutoProvisioner(opts);
            prov.EnsureProvisioned();
            return prov;
        });

        // Background flush service
        services.AddHostedService(provider =>
        {
            var ch = provider.GetRequiredService<System.Threading.Channels.Channel<LogItem>>();
            var writer = provider.GetRequiredService<SqlEventWriter>();
            var opts = provider.GetRequiredService<IOptions<LoggingOptions>>().Value;
            return new BackgroundFlushService(ch, writer, opts.FlushIntervalMs, opts.BulkSize);
        });

        return services;
    }
}