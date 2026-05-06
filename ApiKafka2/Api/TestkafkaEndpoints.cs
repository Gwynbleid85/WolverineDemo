using CleanResult;
using Wolverine;
using Wolverine.Http;
using Ping = SharedKernel.Core.Events.Ping;

namespace ApiKafka2.Api;

public class TestkafkaEndpoints
{
    [WolverinePost("/test/kafka/ping2")]
    public async Task<Result> Ping(IMessageBus bus)
    {
        ContextLog.Information("Sending Ping from Kafka2");
        var ping = new Ping("Ping message 2");
        await bus.PublishAsync(ping);
        return Result.Ok();
    }
}
