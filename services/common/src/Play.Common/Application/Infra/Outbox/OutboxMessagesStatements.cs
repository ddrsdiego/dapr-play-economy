namespace Play.Common.Application.Infra.Outbox;

internal static class OutboxMessagesStatements
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
         )
        VALUES (@Id, @EventName, @TopicName, @FullName, @Payload::jsonb, @Status, @CreatedAt, 'OUT', @PubSubName)
    ";

    public const string SaveFollowUpAsync =
        "INSERT INTO public.\"BoxMessagesFollowUp\"( \"BoxMessagesId\", \"Status\", \"UpdatedAt\", \"Exception\" ) VALUES (@BoxMessagesId, @Status, @UpdatedAt, @Exception)";

    public const string GetUnprocessedAsync = @"
        WITH locked_row AS (
            SELECT * FROM ""BoxMessages"" WHERE ""Status"" = 'Pending' AND ""ProcessorId"" IS NULL FOR UPDATE
        )
        UPDATE ""BoxMessages"" SET
            ""Status"" = 'Processing'
            ,""ProcessorId"" = @ProcessorId
        FROM locked_row
        WHERE ""BoxMessages"".""Id"" = locked_row.""Id""
        RETURNING ""BoxMessages"".*";


    public const string UpdateToPublishedAsync =
        "UPDATE \"BoxMessages\" SET \"Status\" = @Status, \"SentAt\" = @SentAt WHERE \"Id\" = @Id";

    public const string IncrementNumberAttempts =
        "UPDATE \"BoxMessages\" SET \"NumberAttempts\" = @NumberAttempts WHERE \"Id\" = @Id";
}