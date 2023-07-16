using NServiceBus.Features;
using NServiceBus.Logging;

sealed class RateLimiterFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        if (!context.Settings.TryGet<RateLimiterConfiguration>(out var properties)) return;
        LogManager.GetLogger("RateLimiter").InfoFormat("Rate limiter configuration: Limit:{0:N0} Duration:{1:g}", properties.Limit, properties.Duration);
        context.Pipeline.Register(behavior: new RateLimitBehavior(properties.Limit, properties.Duration, properties.StartDurationThreshold), description: nameof(RateLimitBehavior));
    }
}