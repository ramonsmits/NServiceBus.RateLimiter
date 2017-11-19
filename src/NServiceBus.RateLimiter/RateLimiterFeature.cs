using System;
using NServiceBus.Features;

class RateLimiterFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        if (!context.Settings.TryGet<Properties>(out var properties)) return;

        context.Pipeline.Register(behavior: new RateLimitBehavior(properties.Limit, properties.Duration), description: nameof(RateLimitBehavior));
    }
}