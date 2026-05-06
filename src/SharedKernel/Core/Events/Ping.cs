using SharedKernel.Infrastructure.Utils;

namespace SharedKernel.Core.Events;

[KafkaTopic("demo-test-topic")]
public record Ping(string Message) : IKafkaMessage;
