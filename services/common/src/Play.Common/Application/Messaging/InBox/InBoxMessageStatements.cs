namespace Play.Common.Application.Messaging.InBox;

internal static class InBoxMessageStatements
{
    public const string CommitMessage = @"
		UPDATE public.""InBoxMessage"" SET
			""Status"" = @Status
			,""CompletedAt"" = (CURRENT_TIMESTAMP AT TIME ZONE 'America/Sao_Paulo'::text)
			,""TotalDurationInMs"" = ((EXTRACT(EPOCH FROM (""InBoxMessage"".""CompletedAt"" - ""InBoxMessage"".""CreatedAt"")) * 1000)::INTEGER)
		WHERE ""MessageId"" = @MessageId
	";

    public const string GetMessagesWithPriorityPending = @"
		WITH InBoxMessageLocked AS (
			SELECT * FROM public.""InBoxMessage""
			WHERE
				""CompletedAt"" IS NULL
				AND ""Status"" = 'Pending'
				AND ""NumberAttempts"" < @NumberAttempts
				AND ""Priority"" = @Priority
			ORDER BY ""CreatedAt""
			FOR UPDATE SKIP LOCKED
			LIMIT @BatchSize)
		UPDATE public.""InBoxMessage"" SET
			""Status"" = @NextStatus
			,""ProcessorId"" = @ProcessorId
			,""BlockedAt"" = (CURRENT_TIMESTAMP AT TIME ZONE 'America/Sao_Paulo'::text)
		FROM InBoxMessageLocked
		WHERE public.""InBoxMessage"".""MessageId"" = InBoxMessageLocked.""MessageId""
		RETURNING public.""InBoxMessage"".*
	";

    public const string GetPendingMessages = @"
		WITH InBoxMessageLocked AS (
			SELECT * FROM public.""InBoxMessage""
			WHERE
				1 = 1
				AND (
						""Status"" = 'Pending' AND ""CompletedAt"" IS NULL AND ""ProcessorId"" IS NULL AND ""NumberAttempts"" < @NumberAttempts
						OR ""Status"" = 'InProgress' AND ""BlockedAt"" < (CURRENT_TIMESTAMP AT TIME ZONE 'America/Sao_Paulo'::text) - INTERVAL '15 minutes' AND ""CompletedAt"" IS NULL AND ""NumberAttempts"" < @NumberAttempts
				)
			ORDER BY ""Priority"" DESC, ""CreatedAt""			
			FOR UPDATE SKIP LOCKED
			LIMIT @BatchSize)
		UPDATE public.""InBoxMessage"" SET
			""Status"" = @Status
			,""ProcessorId"" = @ProcessorId
			,""BlockedAt"" = (CURRENT_TIMESTAMP AT TIME ZONE 'America/Sao_Paulo'::text)
		FROM InBoxMessageLocked
		WHERE public.""InBoxMessage"".""MessageId"" = InBoxMessageLocked.""MessageId""
		RETURNING public.""InBoxMessage"".*
	";

    public const string IncrementNumberAttempts = @"
		UPDATE public.""InBoxMessage"" SET
			""Status"" = @Status
			,""ProcessorId"" = NULL
			,""BlockedAt"" = NULL
			,""CompletedAt"" = NULL
			,""CreatedAt"" = ""CreatedAt"" + INTERVAL '10 minute'
			,""NumberAttempts"" = @NumberAttempts
		WHERE ""MessageId"" = @MessageId
	";

    public const string RegisterFollowUp = @"
		INSERT INTO public.""InBoxMessageFollowUp"" (""InBoxMessageFollowUpId"", ""MessageId"", ""Status"", ""UpdateAt"", ""Exception"")
		VALUES (
			@InBoxMessageFollowUpId
			,@MessageId
			,@Status
			,@UpdateAt
			,@Exception
		)
	";

    public const string CleanUp = @"
		UPDATE public.""InBoxMessage"" SET
			""Status"" = @Status
			,""CompletedAt"" = NULL
			,""NumberAttempts"" = 0
		WHERE ""BlockedAt"" < NOW() - INTERVAL '5 MINUTE'
	";
}