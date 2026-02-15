using System;
using System.Diagnostics;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

sealed class RateLimitBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
{
    readonly ILog Log = LogManager.GetLogger("RateLimiter");
    readonly bool IsInfoEnabled;
    readonly TimeSpan StartDurationThreshold;
    readonly TokenBucketRateLimiter Limiter;

    public RateLimitBehavior(int occurrences, TimeSpan duration, TimeSpan warningThreshold)
    {
        Limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = occurrences,
            TokensPerPeriod = occurrences,
            ReplenishmentPeriod = duration,
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true,
        });
        StartDurationThreshold = warningThreshold;
        IsInfoEnabled = Log.IsInfoEnabled;
    }

    public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
    {
        long startTimestamp = 0;
        if (IsInfoEnabled && StartDurationThreshold > TimeSpan.Zero)
        {
            startTimestamp = Stopwatch.GetTimestamp();
        }

        using var lease = await Limiter.AcquireAsync(1, context.CancellationToken).ConfigureAwait(false);

        if (startTimestamp > 0)
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            if (elapsed > StartDurationThreshold)
            {
                Log.InfoFormat(
                    "Message '{0}' processing delayed due to throttling by {1:g} which exceeds the configured `" + nameof(RateLimiterConfiguration.StartDurationThreshold) + "` value {2:g}. This can cause issues with message lease times or transaction timeouts. Consider lowering the burst size, shorten the rate limiting duration, reducing the allowed concurrenty, or transport prefetching",
                    context.Message.MessageId,
                    elapsed,
                    StartDurationThreshold
                );
            }
        }

        await next(context).ConfigureAwait(false);
    }
}