namespace SharedKernel.Infrastructure.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class KafkaTopicAttribute(string topicName) : Attribute
{
    public string TopicName { get; } = topicName;
}

public interface IKafkaMessage;
