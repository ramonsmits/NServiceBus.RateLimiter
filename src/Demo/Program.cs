var cfg = new EndpointConfiguration("RateLimiterDemo");
cfg.UseSerialization(new SystemJsonSerializer());
cfg.UseTransport(new LearningTransport());
cfg.LimitMessageProcessingConcurrencyTo(100);   // Increase concurrency so the transport will fetch many messages

cfg.ApplyRateLimiting(c =>
{
    // One message every 250ms
    c.Duration = TimeSpan.FromMilliseconds(250);
    c.Limit = 1;
    c.StartDurationThreshold = TimeSpan.FromSeconds(1);
});

var instance = await Endpoint.Start(cfg);

var tasks = new List<Task>();
for (int i = 0; i < 25; i++)
{
    tasks.Add(instance.SendLocal(new MyMessage()));
}
await Task.WhenAll(tasks);

Console.ReadKey();

await instance.Stop();


class MyMessage : IMessage
{
}

class MyHandler : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        return Console.Out.WriteLineAsync("Received a message");
    }
}
