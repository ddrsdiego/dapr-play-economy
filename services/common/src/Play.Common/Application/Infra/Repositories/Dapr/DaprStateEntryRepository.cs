namespace Play.Common.Application.Infra.Repositories.Dapr;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using global::Dapr.Client;

public abstract class DaprStateEntryRepository<TEntry> : IDaprStateEntryRepository<TEntry>
    where TEntry : IDaprStateEntry
{
    private readonly DaprClient _daprClient;

    protected DaprStateEntryRepository(string stateStoreName, DaprClient daprClient)
    {
        StateStoreName = stateStoreName;
        _daprClient = daprClient;
    }

    public static readonly JsonSerializerOptions JsonSerializerOptions;

    static DaprStateEntryRepository()
    {
        JsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private string StateStoreName { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UpsertAsync(TEntry entity, CancellationToken cancellationToken = default) =>
        UpsertAsync(new[] {entity}, cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UpsertAsync(TEntry[] entities, CancellationToken cancellationToken = default) =>
        InternalUpsertAsync(entities, cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<TEntry>> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        //TODO: Melhorar mensagem de erro e cache miss
        var result = await GetCustomerByIdAsync(new[] {id}, cancellationToken);
        return result.Count == 0 ? Result.Failure<TEntry>("Item Not Found") : result[id];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyDictionary<string, Result<TEntry>>> GetCustomerByIdAsync(string[] ids,
        CancellationToken cancellationToken = default) =>
        InternalGetByIdAsync(ids, cancellationToken);

    private Task<IReadOnlyDictionary<string, Result<TEntry>>> InternalGetByIdAsync(IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (!StateEntryNameManager.TryExtractName<TEntry>(out var stateEntryName))
            stateEntryName = typeof(TEntry).Name;

        var keys = new List<StateItemQueryParameter<TEntry>>(ids.Count);
        for (var index = 0; index < ids.Count; index++)
        {
            keys.Add(new StateItemQueryParameter<TEntry>(stateEntryName, ids[index]));
        }

        var bulkStateItems = _daprClient.GetBulkStateItemsByKeys(StateStoreName, keys, cancellationToken);
        var results = new Dictionary<string, Result<TEntry>>(ids.Count);

        foreach (var bulkStateItem in bulkStateItems)
        {
            var key = keys.Single(x => x.FormattedKey == bulkStateItem.Key);
            if (bulkStateItem.CheckIfValueIsNotEmpty(results, key.OriginalKey))
                continue;

            if (bulkStateItem.TryDeserializeValue(key.OriginalKey, results, out var entity))
                continue;

            results.Add(key.OriginalKey, Result.Success(entity));
        }

        return Task.FromResult<IReadOnlyDictionary<string, Result<TEntry>>>(new ReadOnlyDictionary<string, Result<TEntry>>(results));
    }

    private Task InternalUpsertAsync(IReadOnlyList<TEntry> entities, CancellationToken cancellationToken)
    {
        var requests = new List<StateTransactionRequest>(entities.Count);

        if (!StateEntryNameManager.TryExtractName<TEntry>(out var stateEntryName))
            stateEntryName = typeof(TEntry).Name;

        for (var index = 0; index < entities.Count; index++)
        {
            var entity = entities[index];

            var value = JsonSerializer.SerializeToUtf8Bytes(entity, JsonSerializerOptions);
            var key = KeyFormatterHelper.ConstructStateStoreKey(stateEntryName, entity.StateEntryKey);

            requests.Add(new StateTransactionRequest(key, value, StateOperationType.Upsert));
        }

        var task = _daprClient.ExecuteStateTransactionAsync(StateStoreName, requests,
            cancellationToken: cancellationToken);

        return task.IsCompletedSuccessfully ? Task.CompletedTask : SlowExecute(task);

        static async Task SlowExecute(Task task) => await task;
    }
}