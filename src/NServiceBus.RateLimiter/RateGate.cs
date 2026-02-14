using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Used to control the rate of some occurrence per unit of time.
/// </summary>
/// <remarks>
///     <para>
///     To control the rate of an action using a <see cref="RateGate"/>,
///     code should simply call <see cref="WaitAsync()"/> prior to
///     performing the action. <see cref="WaitAsync()"/> will block
///     the current thread until the action is allowed based on the rate
///     limit.
///     </para>
///     <para>
///     This class is thread safe. A single <see cref="RateGate"/> instance
///     may be used to control the rate of an occurrence across multiple
///     threads.
///     </para>
/// </remarks>
sealed class RateGate : IDisposable
{
    static readonly long TicksMilliseconds = Stopwatch.Frequency / 1000;
    static readonly TimeSpan DisablePeriodicSignaling = TimeSpan.FromMilliseconds(-1);

    readonly SemaphoreSlim _semaphore;
    readonly ConcurrentQueue<long> _exitTimes;
    readonly Timer _exitTimer;
    bool _isDisposed;

    /// <summary>
    /// Number of occurrences allowed per unit of time.
    /// </summary>
    public int Occurrences { get; }

    /// <summary>
    /// The length of the time unit, in milliseconds.
    /// </summary>
    public long TimeUnitTicks { get; }

    /// <summary>
    /// Initializes a <see cref="RateGate"/> with a rate of <paramref name="occurrences"/>
    /// per <paramref name="timeUnit"/>.
    /// </summary>
    /// <param name="occurrences">Number of occurrences allowed per unit of time.</param>
    /// <param name="timeUnit">Length of the time unit.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="occurrences"/> or <paramref name="timeUnit"/> is negative.
    /// </exception>
    public RateGate(int occurrences, TimeSpan timeUnit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(occurrences);
        if (timeUnit != timeUnit.Duration())
            throw new ArgumentOutOfRangeException(nameof(timeUnit), "Time unit must be a positive span of time");
        if (timeUnit >= TimeSpan.FromMilliseconds(uint.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(timeUnit), "Time unit must be less than 2^32 milliseconds");

        Occurrences = occurrences;
        TimeUnitTicks = (long)(Stopwatch.Frequency * timeUnit.TotalSeconds);
        _semaphore = new SemaphoreSlim(Occurrences, Occurrences);
        _exitTimes = new ConcurrentQueue<long>();
        _exitTimer = new Timer(ExitTimerCallback, null, timeUnit, DisablePeriodicSignaling);
    }

    void ExitTimerCallback(object state)
    {
        while (true)
        {
            while (_exitTimes.TryPeek(out var exitTime) && (exitTime - Stopwatch.GetTimestamp()) <= 0)
            {
                _semaphore.Release();
                _exitTimes.TryDequeue(out _);
            }

            long ticksUntilNextCheck = _exitTimes.TryPeek(out var nextExitTime)
                ? nextExitTime - Stopwatch.GetTimestamp()
                : TimeUnitTicks;

            var dueInMilliseconds = ticksUntilNextCheck / TicksMilliseconds;

            if (dueInMilliseconds > 0)
            {
                _exitTimer.Change(dueInMilliseconds, -1);
                break;
            }
        }
    }

    /// <summary>
    /// Blocks the current thread until allowed to proceed or until the
    /// specified timeout elapses.
    /// </summary>
    /// <param name="millisecondsTimeout">Number of milliseconds to wait, or -1 to wait indefinitely.</param>
    /// <param name="cancellationToken">The System.Threading.CancellationToken to observe.</param>
    /// <returns>true if the thread is allowed to proceed, or false if timed out</returns>
    public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsTimeout, -1);
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var entered = await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken)
            .ConfigureAwait(false);

        if (entered)
        {
            _exitTimes.Enqueue(Stopwatch.GetTimestamp() + TimeUnitTicks);
        }

        return entered;
    }

    /// <summary>
    /// Blocks the current thread until allowed to proceed or until the
    /// specified timeout elapses.
    /// </summary>
    /// <param name="timeout">A System.TimeSpan that represents the number of milliseconds to wait, a System.TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
    /// <param name="cancellationToken">The System.Threading.CancellationToken to observe.</param>
    /// <returns>true if the thread is allowed to proceed, or false if timed out</returns>
    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);
    }

    /// <summary>
    /// Blocks the current thread indefinitely until allowed to proceed.
    /// </summary>
    public Task WaitAsync()
    {
        return WaitAsync(Timeout.Infinite, CancellationToken.None);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _semaphore.Dispose();
        _exitTimer.Dispose();
        _isDisposed = true;
    }
}
