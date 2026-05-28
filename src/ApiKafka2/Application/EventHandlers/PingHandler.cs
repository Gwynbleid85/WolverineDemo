using SharedKernel.Core.Events;

namespace ApiKafka2.Application.EventHandlers;

public class PingHandler
{
    public static Pong Handle(Ping externalEvent)
    {
        ContextLog.Information("Received Ping in Kafka2");
        return new Pong("Pong message 2");
    }
}
