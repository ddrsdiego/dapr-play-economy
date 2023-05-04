namespace Play.Common.Application.Messaging;

using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Infra.Repositories;
using OutBox;

public abstract class BoxMessagesRepository
{
    protected BoxMessagesRepository(IConnectionManager connectionManager)
    {
        ConnectionManager = connectionManager;
        Processor = new MessagesProcessorId(Guid.NewGuid().ToString());
    }

    protected readonly MessagesProcessorId Processor;
    
    protected readonly IConnectionManager ConnectionManager;

    protected static Task RegisterFollowUpAsync(IDbConnection conn, BoxMessage outboxMessagePublished, string status,
        string errorMessage = "")
    {
        const string sql = OutBoxMessagesStatements.SaveFollowUpAsync;
        return conn.ExecuteAsync(sql,
            new
            {
                BoxMessagesId = outboxMessagePublished.MessageId,
                Status = status,
                UpdatedAt = DateTimeOffset.UtcNow,
                Exception = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : null
            });
    }
}