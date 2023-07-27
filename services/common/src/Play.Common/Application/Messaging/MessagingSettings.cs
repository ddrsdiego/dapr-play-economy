namespace Play.Common.Application.Messaging;

public class MessagingSettings
{
    public InBoxMessagingSettings InBox { get; set; }
}

public class InBoxMessagingSettings
{
    public static readonly InBoxMessagingSettings Default = new()
    {
        NumberAttempts = 30,
        BufferCapacity = 1024 * 1024 * 50,
        BatchSize = 1_000,
        IntervalTimeFetchMessage = 1_000,
    };

    public int NumberAttempts { get; set; }
    public int BufferCapacity { get; set; }
    public int BatchSize { get; set; }
    public int IntervalTimeFetchMessage { get; set; }
}