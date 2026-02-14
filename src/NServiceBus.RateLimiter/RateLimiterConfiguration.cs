using System;

/// <summary>
/// Rate limiter configuration
/// </summary>
public sealed class RateLimiterConfiguration
{
    /// <summary>
    /// The duration after which the configured rate Limit is reset.
    /// </summary>
    public TimeSpan Duration { get; set; }
    /// <summary>
    /// The maximum number of message to process within the configured Duration.
    /// </summary>
    public int Limit { get; set; }
    /// <summary>
    /// The duration warning threshold after which a Informational log entry if logged to raise awareness of an imbalance between configured rate limit and concurrency. Set to -1 milliseconds to disable.
    /// </summary>
    public TimeSpan StartDurationThreshold { get; set; } = TimeSpan.FromSeconds(5);
}
