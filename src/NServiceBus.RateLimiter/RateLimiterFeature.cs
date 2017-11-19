using System;
using NServiceBus.Features;

class RateLimiterFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        if (!context.Settings.TryGet<Properties>(out var properties)) return;

        var limit = properties.Concurrency;
        var duration = TimeSpan.FromSeconds((double)limit / properties.Limit);
        context.Pipeline.Register(behavior: new RateLimitBehavior(limit, duration), description: nameof(RateLimitBehavior));
    }
}