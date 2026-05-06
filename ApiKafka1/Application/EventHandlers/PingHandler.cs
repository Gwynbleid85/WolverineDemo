using SharedKernel.Core.Events;

namespace ApiKafka1.Application.EventHandlers;

public class PingHandler
{
    public static void Handle(Ping externalEvent)
    {
        ContextLog.Information("Received Ping in Kafka1");
    }
}
