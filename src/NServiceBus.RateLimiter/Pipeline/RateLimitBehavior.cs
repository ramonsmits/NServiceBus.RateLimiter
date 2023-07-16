using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

sealed class RateLimitBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
{
    readonly ILog Log = LogManager.GetLogger("RateLimiter");
    readonly bool IsInfoEnabled;
    readonly TimeSpan StartDurationThreshold;
    readonly RateGate Gate;

    public RateLimitBehavior(int occurrences, TimeSpan duration, TimeSpan warningThreshold)
    {
        Gate = new RateGate(occurrences, duration);
        StartDurationThreshold = warningThreshold;
        IsInfoEnabled = Log.IsInfoEnabled;
    }

    public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
    {
        if (IsInfoEnabled && StartDurationThreshold > TimeSpan.Zero)
        {
            var start = Stopwatch.StartNew();
            await Gate.WaitAsync().ConfigureAwait(false);
            var duration = start.Elapsed;
            if (duration > StartDurationThreshold)
            {
                Log.InfoFormat(
                    "Message '{0}' processing delayed due to throttling by {1:g} which exceeds the configured `" + nameof(RateLimiterConfiguration.StartDurationThreshold) + "` value {2:g}. This can cause issues with message lease times or transaction timeouts. Consider lowering the burst size, shorten the rate limiting duration, reducing the allowed concurrenty, or transport prefetching",
                    context.Message.MessageId,
                    duration,
                    StartDurationThreshold
                );
            }
        }
        else
        {
            await Gate.WaitAsync().ConfigureAwait(false);
        }
        await next(context).ConfigureAwait(false);
    }
}