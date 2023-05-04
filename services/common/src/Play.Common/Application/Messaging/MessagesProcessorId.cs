namespace Play.Common.Application.Messaging;

public sealed class MessagesProcessorId
{
    public string Value { get; }

    public MessagesProcessorId(string value) => Value = value;
}