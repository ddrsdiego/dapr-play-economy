namespace Play.Common.Application.Infra.Outbox;

internal static class OutboxMessagesStatements
{
    public const string SaveAsync =
        "INSERT INTO public.\"BoxMessages\"(\"Id\", \"EventName\", \"TopicName\", \"FullName\", \"Payload\", \"Status\", \"CreatedAt\", \"BoxType\") VALUES (@Id, @EventName, @TopicName, @FullName, @Payload::jsonb, @Status, @CreatedAt, 'OUT')";

    public const string GetUnprocessedAsync =
        "SELECT * FROM \"BoxMessages\" where \"Status\" = 'Pending' and \"NumberAttempts\" <= 5";

    public const string UpdateToPublishedAsync = "UPDATE \"BoxMessages\" SET \"Status\" = @Status, \"SentAt\" = @SentAt WHERE \"Id\" = @Id";
}