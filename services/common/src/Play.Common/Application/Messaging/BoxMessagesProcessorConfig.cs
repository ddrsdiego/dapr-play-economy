namespace Play.Common.Application.Messaging;

public class BoxMessagesProcessorConfig
{
    private const int DefaultMaxNumberAttempts = 5;
    private const int DefaultExpiryInSeconds = 60;
    private const int DefaultMaxProcessingMessagesCount = 100;
    private const int DefaultProcessingIntervalInSeconds = 30;

    public BoxMessagesProcessorConfig()
    {
        ExpiryInSeconds = DefaultExpiryInSeconds;

        MaxNumberAttempts = DefaultMaxNumberAttempts;
        MaxProcessingMessagesCount = DefaultMaxProcessingMessagesCount;
        ProcessingIntervalInSeconds = DefaultProcessingIntervalInSeconds;
    }

    public string PubSubName { get; init; }
    public string LockStoreName { get; init; }
    public int ProcessingIntervalInSeconds { get; set; }
    public int MaxProcessingMessagesCount { get; set; }
    public int ExpiryInSeconds { get; init; }
    public int MaxNumberAttempts { get; set; }
}