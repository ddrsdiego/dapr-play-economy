namespace Play.Common.Application.Messaging.OutBox;

internal static class OutBoxMessagesStatements
{
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
        VALUES (@Id, @EventName, @TopicName, @FullName, @Payload::jsonb, @Status, @CreatedAt, 'OUT', @PubSubName, @Sender)";

    public const string SaveFollowUpAsync =
        "INSERT INTO public.\"BoxMessagesFollowUp\"( \"BoxMessagesId\", \"Status\", \"UpdatedAt\", \"Exception\" ) VALUES (@BoxMessagesId, @Status, @UpdatedAt, @Exception)";

    public const string GetUnprocessedAsync = @"
        WITH locked_row AS (
            SELECT * FROM ""BoxMessages"" WHERE ""CompletedAt"" IS NULL AND ""NumberAttempts"" <= @NumberAttempts FOR UPDATE
        )
        UPDATE ""BoxMessages"" SET
            ""Status"" = 'Processing'
            ,""ProcessorId"" = @ProcessorId
        FROM locked_row
        WHERE ""BoxMessages"".""Id"" = locked_row.""Id""
        RETURNING ""BoxMessages"".*";


    public const string UpdateToPublishedAsync =
        "UPDATE \"BoxMessages\" SET \"Status\" = @Status, \"CompletedAt\" = @SentAt WHERE \"Id\" = @Id";

    public const string IncrementNumberAttempts =
        "UPDATE \"BoxMessages\" SET \"NumberAttempts\" = @NumberAttempts WHERE \"Id\" = @Id";
}