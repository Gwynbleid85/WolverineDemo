using SharedKernel.Core.Events;

namespace ApiKafka2.Application.EventHandlers;

public class PongHandler
{
    public static void Handle(Pong externalEvent)
    {
        ContextLog.Information("Received Pong in Kafka2");
    }
}
