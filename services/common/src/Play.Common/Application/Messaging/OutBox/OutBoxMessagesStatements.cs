namespace Play.Common.Application.Messaging.OutBox;

internal static class OutBoxMessagesStatements
{
    public const string SaveAsync = @"
        INSERT INTO public.""OutBoxMessages""(
            ""MessageId""
            ,""PubSubName""
            ,""EventName""
            ,""TopicName""
            ,""Status""
            ,""CreatedAt""
            ,""FullName""
            ,""Payload""
            ,""BoxType""
            ,""Sender""
         )
        VALUES (@MessageId, @PubSubName, @EventName, @TopicName, @Status, @CreatedAt, @FullName, @Payload::jsonb, 'OUT', @Sender)";

    public const string SaveFollowUpAsync =
        "INSERT INTO public.\"OutBoxMessagesFollowUp\"( \"MessageId\", \"Status\", \"UpdatedAt\", \"Exception\" ) VALUES (@BoxMessagesId, @Status, @UpdatedAt, @Exception)";

    public const string FetchUnprocessedAsync = @"
        WITH locked_row AS (
            SELECT MessageId
            FROM public.""OutBoxMessages""
            WHERE
                ""CompletedAt"" IS NULL
                AND ""NumberAttempts"" <= @NumberAttempts
            FOR UPDATE SKIP LOCKED
            LIMIT @BatchSize
        )
        UPDATE public.""OutBoxMessages"" SET
            ""Status"" = 'Processing'
            ,""ProcessorId"" = @ProcessorId
        FROM locked_row
        WHERE public.""OutBoxMessages"".""MessageId"" = locked_row.""MessageId""
        RETURNING public.""OutBoxMessages"".*";


    public const string UpdateToPublishedAsync = @"
        UPDATE public.""OutBoxMessages"" SET ""Status"" = @Status, ""CompletedAt"" = @SentAt WHERE ""MessageId"" = @MessageId";
    
    public const string IncrementNumberAttempts =
        "UPDATE \"BoxMessages\" SET \"NumberAttempts\" = @NumberAttempts WHERE \"Id\" = @Id";
}