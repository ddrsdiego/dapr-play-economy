namespace Play.Common.Application.Messaging.InBox;

public static class InBoxMessagesStatements
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
        VALUES (@Id, @EventName, @TopicName, @FullName, @Payload::jsonb, @Status, @CreatedAt, 'IN', @PubSubName, @Sender)";
}