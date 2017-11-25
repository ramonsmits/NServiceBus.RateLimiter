# NServiceBus.RateLimiter

Before NServiceBus 6 there was the ability to throttle the number of messages processed per second. This feature was deprecated because throttling can be conflicting with the different types of tranports and transaction modes. This package restores this functionality.


## Version compatibility

NServiceBus | NServiceBus.RateLimiter
------------|------------------------
v6.x        | v1.x
v7.0        | v2.x

## Use cases

Rate limiting can be used to reduce the pressure on your infrastructure or the infrastructure of others. Lets assume you send out a mass mailing and you do not want to consume a lot of bandwidth or maybe you are calling into a third party services that doesn't allow more then 100 requests per second. Rate limiting can help to make the whole processing more efficient without triggering unneeded retry logic.


## Important

The packages registers itself in the first stage of the pipeline (`ITransportReceiveContext`) this means that when this behavior is called the message is actually already retrieved from the queue. If you use a transactional queue this means that your transport transaction duration will increase (MSMQ, SQL Server, etc.). If you use lease based transport (Azure Storage Queues).

For this reason a warning log entry is written is the delay takes more then 5 seconds.

> Message 'A88A7826-79B3-4203-BB3C-266DE76754F0' delayed by 00:01:15 due to throttling or due to other handlers taking a lot of time to complete.  This can conflict with message lease times or transaction timeouts. Consider lowering the concurrency level or to shorten the rate limiting duration.

Consider lowering the concurrency level as that might cause the rate limit to be reached very fast.


## Installation

Install the Nuget package [NServiceBus.RateLimiter](https://www.nuget.org/packages/NServiceBus.RateLimiter)


## Configuration

### Automatic interval calculation

Calculates the smallest interval based on rate expressed in items per second based on the allowed concurrency.
```
endpointConfiguration.ApplyRateLimiting(int limitPersecond, int concurrency)
```

Limits the processing rate per second based on `limitPersecond`. It will calculate the smallest interval based on the specified concurrency.

#### Example:

If you allow for 100 messages per second, with a concurrency of 4 then the logic will limit processing of 4 messages every 400 milliseconds.

```
endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, concurrency:4);
```


### Manual


Manual control over the rate limit and duration.
```
endpointConfiguration.ApplyRateLimiting(int limit, TimeSpan duration)
```

If you do not want to use the smallest interval, but want to allow for higher burst rates then set concurrency to the max burst size.

#### Example:

If you allow for burst of 250 items as a rate of 10 messages per second then the logic will limit processing of 250 messages every 25 seconds.

```
endpointConfiguration.ApplyRateLimiting(limitPerSecond:10, concurrency:250);
```

Using this logic you can cope with sudden peaks of incoming messages but still maintain - on average - your rate of 10 messages per second.
