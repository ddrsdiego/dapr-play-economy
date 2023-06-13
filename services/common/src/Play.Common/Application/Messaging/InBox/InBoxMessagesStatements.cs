namespace Play.Common.Application.Messaging.InBox;

public static class InBoxMessagesStatements
{
    public const string MarkMessageAsProcessedAsync = @"
        UPDATE ""BoxMessages"" SET ""Status"" = @Status, ""CompletedAt"" = @CompletedAt WHERE ""Id"" = @Id";
    
    public const string SaveAsync = @"
        INSERT INTO public.""BoxMessages""(
            ""Id""
            ,""EventName""
            ,""TopicName""
            ,""FullName""
            ,""Payload""
            ,""Status""
            ,""CreatedAt""
            ,""BoxType""
            ,""PubSubName""
            ,""Sender""
         )
        VALUES (@Id, @EventName, @TopicName, @FullName, @Payload::jsonb, @Status, @CreatedAt, 'IN', @PubSubName, @Sender)";

    public const string GetUnprocessedMessagesAsync = @"
        WITH locked_row AS (
            SELECT * FROM ""BoxMessages"" WHERE ""CompletedAt"" IS NULL AND ""NumberAttempts"" <= @NumberAttempts AND ""BoxType"" = 'IN' FOR UPDATE LIMIT @BatchSize
        )
        UPDATE ""BoxMessages"" SET
            ""Status"" = 'Processing'
            ,""ProcessorId"" = @ProcessorId
        FROM locked_row
        WHERE ""BoxMessages"".""Id"" = locked_row.""Id""
        RETURNING ""BoxMessages"".*";
}