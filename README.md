# NServiceBus.RateLimiter

Before NServiceBus 6 there was the ability to throttle the number of messages processed per second. This feature was deprecated because throttling can be conflicting with the different types of tranports and transaction modes. This package restores and improves this functionality.


## Version compatibility

NServiceBus | NServiceBus.RateLimiter
------------|------------------------
v6.x        | v1.x
v7.x        | v2.x
v8.x        | v3.x

## Use cases

Rate limiting can be used to reduce the pressure on your infrastructure or the infrastructure of others. Lets assume you send out a mass mailing and you do not want to consume a lot of bandwidth or maybe you are calling into a third party services that doesn't allow more then 100 requests per second. Rate limiting can help to make the whole processing more efficient without triggering unneeded retry logic.


## Important

The packages registers itself in the first stage of the pipeline (`ITransportReceiveContext`) this means that when this behavior is called the message is actually already retrieved from the queue. If you use a transactional queue this means that your transport transaction duration will increase (MSMQ, SQL Server, etc.). If you use lease based transport (Azure Service Bus or Azure Storage Queues) the lease duration might conflict with the processing delay introduced and unintentionally cause more-than-once processing.

For this reason a warning log entry is written when the delay takes more then 5 seconds.

> Message 'A88A7826-79B3-4203-BB3C-266DE76754F0' delayed by 00:01:15 due to throttling or due to other handlers taking a lot of time to complete.  This can conflict with message lease times or transaction timeouts. Consider lowering the burst size or to shorten the rate limiting duration.

Consider lowering the burst size as that might cause the rate limit to be reached very fast.

## Recommendations

It is recommended to:

1. Disable prefetching if a transport supports this. Prefetching doesn't make sense if the processing gets delayed.
2. Lower the [concurrency limit which by default is equal the the number of cores of the machine](https://docs.particular.net/nservicebus/operations/tuning)

## Installation

Install the Nuget package [NServiceBus.RateLimiter](https://www.nuget.org/packages/NServiceBus.RateLimiter)

## Configuration

### Example: Automatic interval calculation

Calculates the smallest interval based on rate expressed in items per second based on the allowed concurrency.

```c#
endpointConfiguration.ApplyRateLimiting(int limitPersecond)
```

Limits the processing rate per second based on `limitPersecond`.

### Example: Specify maximum burst size

If you allow for 100 messages per second, with a concurrency of 4 then the logic will limit processing of 4 messages every 400 milliseconds.

```c#
endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, burstSize:4);
```

NOTE: The burst window duration must be larger than 100 milliseconds. The `burstSize` argument must always be atleast 1/10th the size of `limitPerSecond`.

### Example: Manually enter a limit and burst window duration


Manual control over the rate limit and duration.

```c#
endpointConfiguration.ApplyRateLimiting(int limit, TimeSpan duration)
```

If you do not want to use the smallest interval, but want to allow for higher burst rates then set concurrency to the max burst size.

### Example: Large bursts

To allow bursts of 250 items at an average rate of 10 messages per second just enter the max burst size. This will internally calculate the corresponding burst window duration. Here this would be `250 messages / 10 messages per second  = 25 seconds`.

```c#
endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, burstSize:250);
```

Using this logic to cope with sudden peaks of incoming messages but still maintain an average  rate of messages per second.

### Example: Disable duration log entry message

By default, is processing does not start within 5 seconds a message is written to the log. This can be disabled.

```c#
endpointConfiguration.ApplyRateLimiting(cfg=>{
  cfg.Duration = TimeSpan.FromSeconds(1),
  cfg.Limit = 10,
  cfg.StartDurationThreshold = Timeout.InfiniteTimeSpan; // or TimeSpan.FromMilliseconds(-1);
cfg.
});
```