namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Play.Common.Application.Infra.Repositories;
using Play.Common.Application.Messaging.InBox.Extensions;

public interface IInBoxMessageStorageWrite
{
    Task IncrementRetryNumberAttemptsAsync(InBoxMessage inBoxMessage, string errorMessage, CancellationToken cancellationToken = default);

    Task CommitMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default);

    Task SaveAsync(ArraySegment<InBoxMessage> inBoxMessages, CancellationToken cancellationToken = default);
}

public sealed class InBoxMessagePostgresStorageWrite : PostgresRepository, IInBoxMessageStorageWrite
{
    private static readonly SemaphoreSlim SaveLockSemaphore = new(1, 1);
    private static readonly SemaphoreSlim MarkMessageAsCompletedSemaphore = new(1, 1);

    public InBoxMessagePostgresStorageWrite(ILoggerFactory logger, IConnectionManagerFactory connectionManager)
        : base(logger.CreateLogger<InBoxMessagePostgresStorageWrite>(), connectionManager)
    {
    }

    public async Task IncrementRetryNumberAttemptsAsync(InBoxMessage inBoxMessage, string errorMessage, CancellationToken cancellationToken = default)
    {
        const string failedStatusFollowUp = "Failed";
        const string sqlIncrementNumberAttempts = InBoxMessageStatements.IncrementNumberAttempts;

        try
        {
            inBoxMessage.IncrementNumberAttempts();

            await using var connectionManager = ConnectionManagerFactory.CreateConnection();
            await using var conn = await connectionManager.GetOpenConnectionAsync(cancellationToken);

            await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(sqlIncrementNumberAttempts, new { inBoxMessage.MessageId, inBoxMessage.NumberAttempts, Status = InBoxMessage.InBoxMessageStatus.Pending });
            await RegisterFollowUpAsync(new[] { inBoxMessage }, conn, failedStatusFollowUp, errorMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task CommitMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default)
    {
        const string completedStatusFollowUp = "Completed";
        const string sql = InBoxMessageStatements.CommitMessage;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await MarkMessageAsCompletedSemaphore.WaitAsync(cancellationToken);

            inBoxMessage.UpdateStatus(InBoxMessage.InBoxMessageStatus.Completed);

            await using var connectionManager = ConnectionManagerFactory.CreateConnection();
            await using var conn = await connectionManager.GetOpenConnectionAsync(cancellationToken);

            await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(sql, new { inBoxMessage.MessageId, inBoxMessage.Status });
            await RegisterFollowUpAsync(new[] { inBoxMessage }, conn, completedStatusFollowUp);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            MarkMessageAsCompletedSemaphore.Release();
        }
    }

    public async Task SaveAsync(ArraySegment<InBoxMessage> inBoxMessages, CancellationToken cancellationToken = default)
    {
        const string receivedStatusFollowUp = "Received";

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await SaveLockSemaphore.WaitAsync(cancellationToken);
            var p = inBoxMessages.OrderByDescending(x => x.Priority).ToArray();

            var messages = new InBoxMessage[p.Length];
            for (var index = 0; index < p.Length; index++)
            {
                messages[index] = p[index];
            }

            var sql = messages.ToSaveQuery();
            var parameters = messages.ToSaveParameters();

            await using var connectionManager = ConnectionManagerFactory.CreateConnection();
            await using var conn = await connectionManager.GetOpenConnectionWithTransactionAsync(cancellationToken);

            await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(sql, parameters);
            await RegisterFollowUpAsync(messages, conn, receivedStatusFollowUp);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            SaveLockSemaphore.Release();
        }
    }

    private static Task RegisterFollowUpAsync(InBoxMessage[] inBoxMessages, IDbConnection conn, string status, string errorMessage = null)
    {
        var sql = inBoxMessages.ToRegisterFollowUpQuery();
        var parameters = inBoxMessages.ToRegisterFollowUpParameters(status, errorMessage);
        return conn.ExecuteAsync(sql, parameters);
    }

    private async Task CleanUpStaleLocksAsync(CancellationToken cancellationToken = default)
    {
        const string sql = InBoxMessageStatements.CleanUp;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var timeToLiveToBlocked = TimeSpan.FromMinutes(5);

                await using var connectionManager = ConnectionManagerFactory.CreateConnection();
                await using var conn = await connectionManager.GetOpenConnectionAsync(cancellationToken);

                await SetDataBaseTimeZoneAsync(conn, cancellationToken: cancellationToken);
                await conn.ExecuteAsync(sql, new { Status = InBoxMessage.InBoxMessageStatus.Pending, TimeToLiveToBlocked = timeToLiveToBlocked.TotalMinutes });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1_000 * 60 * 30), cancellationToken);
        }
    }
}