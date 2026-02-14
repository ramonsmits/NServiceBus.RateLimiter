using System;
using System.Threading;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus;

public static class RateLimiterConfigurationExtension
{
    static readonly TimeSpan MinimumDuration = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Limits the processing rate per second based on <param name="limitPerSecond"/>. It will calculate the smallest interval based on the specified concurrency.
    ///
    /// If you do not want to use the smallest interval but want to allow for burst then set concurrency to the max burst size.
    ///
    /// Smallest interval based on concurrency:
    /// If you allow for 100 messages per second, with a concurrency of 4 then the logic will limit processing of 4 messages every 400 milliseconds.
    ///
    ///     endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, concurrency:4);
    ///
    /// Bursts:
    /// If you allow for burst of 250 items as a rate of 10 messages per second then the logic will limit processing of 250 messages every 25 seconds.
    ///
    ///     endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, concurrency:250);
    /// </summary>
    public static void ApplyRateLimiting(this EndpointConfiguration instance, int limitPerSecond, int burstSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limitPerSecond, 1);
        var minimumBurstSize = limitPerSecond / 10D;
        if (burstSize < minimumBurstSize) throw new ArgumentOutOfRangeException(nameof(burstSize), burstSize, $"Must be larger than {Math.Ceiling(minimumBurstSize):N0} when {nameof(limitPerSecond)}={limitPerSecond}");
        var properties = new global::RateLimiterConfiguration
        {
            Duration = TimeSpan.FromSeconds(1.0 / limitPerSecond * burstSize),
            Limit = burstSize,
        };
        ApplyConfiguration(instance, properties);
    }

    /// <summary>
    /// If you want full control over the limit and the duration then this overload allows to set any rates.
    ///
    /// Minute max rates:   100 calls every minute:
    ///
    ///     endpointConfiguration.ApplyRateLimiting(100, TimeSpan.FromMinutes(1));
    ///
    /// </summary>
    public static void ApplyRateLimiting(this EndpointConfiguration instance, int limit, TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        if (duration < MinimumDuration) throw new ArgumentOutOfRangeException(nameof(duration), duration, $"Must be larger than {MinimumDuration.TotalMilliseconds}ms");
        var properties = new global::RateLimiterConfiguration
        {
            Duration = duration,
            Limit = limit,
        };
        ApplyConfiguration(instance, properties);
    }

    /// <summary>
    /// If you want full control over the limit and the duration then this overload allows to set any rates.
    ///
    /// Max rate:   5 calls every second:
    ///
    ///     endpointConfiguration.ApplyRateLimiting(5);
    ///
    /// </summary>
    public static void ApplyRateLimiting(this EndpointConfiguration instance, int limitPerSecond)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limitPerSecond, 1);
        var properties = new global::RateLimiterConfiguration
        {
            Duration = TimeSpan.FromSeconds(1),
            Limit = limitPerSecond,
        };
        ApplyConfiguration(instance, properties);
    }

    /// <summary>
    /// Enable rate limiting.
    /// </summary>
    public static void ApplyRateLimiting(this EndpointConfiguration instance, Action<RateLimiterConfiguration> config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var properties = new RateLimiterConfiguration();
        config(properties);

        ArgumentOutOfRangeException.ThrowIfLessThan(properties.Limit, 1, nameof(RateLimiterConfiguration.Limit));
        if (properties.Duration < MinimumDuration) throw new ArgumentOutOfRangeException(nameof(RateLimiterConfiguration.Duration), properties.Duration, $"Must be larger than {MinimumDuration.TotalMilliseconds}ms");
        if (properties.StartDurationThreshold == TimeSpan.Zero || properties.StartDurationThreshold < Timeout.InfiniteTimeSpan) throw new ArgumentOutOfRangeException(nameof(RateLimiterConfiguration.StartDurationThreshold), properties.StartDurationThreshold, $"Must be larger than 0 or -1ms to disable");

        ApplyConfiguration(instance, properties);
    }

    static void ApplyConfiguration(EndpointConfiguration instance, RateLimiterConfiguration properties)
    {
        instance.GetSettings().Set(properties);
        instance.EnableFeature<RateLimiterFeature>();
    }
}
