using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

class RateLimitBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
{
    readonly ILog Log = LogManager.GetLogger("RateLimiter");
    readonly TimeSpan WarningThresshold = TimeSpan.FromSeconds(5);
    readonly RateGate Gate;

    public RateLimitBehavior(int occurences, TimeSpan duration)
    {
        Gate = new RateGate(occurences, duration);
    }

    public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
    {
        var start = Stopwatch.StartNew();
        await Gate.WaitToProceed().ConfigureAwait(false);
        var duration = start.Elapsed;
        if (duration > WarningThresshold)
        {
            Log.InfoFormat("Message '{0}' delayed by {1:g} due to throttling. This can conflict with message lease times or transaction timeouts. Consider lowering the concurrency level or to shorten the rate limiting duration.",
                context.Message.MessageId,
                duration
            );
        }
        await next(context).ConfigureAwait(false);
    }
}