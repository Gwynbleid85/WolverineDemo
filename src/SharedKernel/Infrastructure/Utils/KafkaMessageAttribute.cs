namespace SharedKernel.Infrastructure.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class KafkaMessageAttribute(string topicName) : Attribute
{
    public string TopicName { get; init; } = topicName;
}
