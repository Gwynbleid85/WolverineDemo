using CleanResult;
using Wolverine;
using Wolverine.Http;
using Ping = SharedKernel.Core.Events.Ping;

namespace ApiKafka1.Api;

public class TestkafkaEndpoints
{
    [WolverinePost("/test/kafka/ping1")]
    public async Task<Result> Ping2(IMessageBus bus)
    {
        ContextLog.Information("Sending Ping from Kafka1");
        var ping = new Ping("Ping message 1");
        await bus.PublishAsync(ping);
        return Result.Ok();
    }
}
