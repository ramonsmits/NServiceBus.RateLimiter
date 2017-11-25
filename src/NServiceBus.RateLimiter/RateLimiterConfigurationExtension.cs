using System;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus
{
    public static class RateLimiterConfigurationExtension
    {

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
        public static void ApplyRateLimiting(this EndpointConfiguration instance, int limitPerSecond, int concurrency)
        {
            var properties = new global::Properties
            {
                Duration = TimeSpan.FromSeconds(1.0 / limitPerSecond * concurrency),
                Limit = concurrency,
            };
            var settings = instance.GetSettings();
            settings.Set<global::Properties>(properties);
            instance.EnableFeature<RateLimiterFeature>();
        }

        /// <summary>
        /// If you want full control over the limit and the duration then this overload allows to set any rates.
        /// 
        /// Minute max rates:   100 calls every minute:
        /// 
        ///     endpointConfiguration.ApplyRateLimiting(100, TimeSpan.FromMinutes(1));
        /// 
        /// 
        /// 
        /// </summary>
        public static void ApplyRateLimiting(this EndpointConfiguration instance, int limit, TimeSpan duration)
        {
            var properties = new global::Properties
            {
                Duration = duration,
                Limit = limit,
            };
            var settings = instance.GetSettings();
            settings.Set<global::Properties>(properties);
            instance.EnableFeature<RateLimiterFeature>();
        }
    }
}
