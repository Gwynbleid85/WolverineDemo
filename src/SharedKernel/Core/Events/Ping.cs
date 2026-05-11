using SharedKernel.Infrastructure.Utils;

namespace SharedKernel.Core.Events;

[KafkaMessage("demo-test-topic")]
public record Ping(string Message);
