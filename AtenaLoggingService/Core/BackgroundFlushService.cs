using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AtenaLoggingService.Core;

internal sealed class BackgroundFlushService : BackgroundService
{
    private readonly Channel<LogItem> _channel;
    private readonly SqlEventWriter _writer;
    private readonly int _flushIntervalMs;
    private readonly int _bulkSize;

    public BackgroundFlushService(Channel<LogItem> channel, SqlEventWriter writer, int flushIntervalMs, int bulkSize)
    {
        _channel = channel;
        _writer = writer;
        _flushIntervalMs = flushIntervalMs;
        _bulkSize = bulkSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<LogItem>(_bulkSize);
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_flushIntervalMs));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_channel.Reader.TryRead(out var item))
                {
                    buffer.Add(item);
                    if (buffer.Count >= _bulkSize)
                    {
                        Flush(buffer);
                    }
                }

                await timer.WaitForNextTickAsync(stoppingToken);

                if (buffer.Count > 0)
                {
                    Flush(buffer);
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (buffer.Count > 0) Flush(buffer);
        }

        void Flush(List<LogItem> buf)
        {
            try
            {
                _writer.WriteBatch(buf);
            }
            catch
            {
                // swallow to never break the app; optionally add a fallback/log counter
            }
            finally
            {
                buf.Clear();
            }
        }
    }
}