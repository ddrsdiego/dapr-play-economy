namespace Play.Common.Application.Infra.Repositories.Dapr
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.Json;
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
        public async Task<Result<TEntry>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await GetByIdAsync(new[] {id}, cancellationToken);
            return result[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IReadOnlyDictionary<string, Result<TEntry>>> GetByIdAsync(string[] ids,
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
            var results = new Dictionary<string, Result<TEntry>>(bulkStateItems.Count);
            foreach (var bulkStateItem in bulkStateItems)
            {
                var key = keys.Single(x => x.FormattedKey == bulkStateItem.Key);
                if (bulkStateItem.CheckIfValueIsNotEmpty(results, key.OriginalKey))
                    continue;

                if (bulkStateItem.TryDeserializeValue(key.OriginalKey, results, out var entity))
                    continue;
                
                results.Add(key.OriginalKey, Result.Success(entity));
            }

            return Task.FromResult<IReadOnlyDictionary<string, Result<TEntry>>>(
                new ReadOnlyDictionary<string, Result<TEntry>>(results));
        }

        private Task InternalUpsertAsync(IReadOnlyCollection<TEntry> entities, CancellationToken cancellationToken)
        {
            var requests = new List<StateTransactionRequest>(entities.Count);

            if (!StateEntryNameManager.TryExtractName<TEntry>(out var stateEntryName))
                stateEntryName = typeof(TEntry).Name;

            foreach (var entity in entities)
            {
                var value = JsonSerializer.SerializeToUtf8Bytes(entity);
                var key = KeyFormatterHelper.ConstructStateStoreKey(stateEntryName, entity.StateEntryKey);

                requests.Add(new StateTransactionRequest(key, value, StateOperationType.Upsert));
            }

            var task = _daprClient.ExecuteStateTransactionAsync(StateStoreName, requests,
                cancellationToken: cancellationToken);

            return task.IsCompletedSuccessfully ? Task.CompletedTask : SlowExecute(task);

            static async Task SlowExecute(Task task) => await task;
        }
    }
}