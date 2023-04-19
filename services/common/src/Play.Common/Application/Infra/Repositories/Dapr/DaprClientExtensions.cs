namespace Play.Common.Application.Infra.Repositories.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Dapr.Client;

    internal static class DaprClientExtensions
    {
        public static IReadOnlyList<BulkStateItem> GetBulkStateItemsByKeys<TEntry>(this DaprClient daprClient,
            string stateStoreName, IEnumerable<StateItemQueryParameter<TEntry>> keys,
            CancellationToken cancellationToken = default)
        {
            var formattedKeys = keys.Select(x => x.FormattedKey).ToList();

            var bulkStateItemsTask = daprClient.GetBulkStateAsync(stateStoreName, formattedKeys,
                parallelism: Environment.ProcessorCount,
                cancellationToken: cancellationToken);

            var bulkStateItems = bulkStateItemsTask.IsCompletedSuccessfully
                ? bulkStateItemsTask.Result
                : SlowQuery(bulkStateItemsTask).Result;

            static async Task<IReadOnlyList<BulkStateItem>> SlowQuery(Task<IReadOnlyList<BulkStateItem>> queryTask)
            {
                var bulkStateItem = await queryTask;
                return bulkStateItem;
            }

            return bulkStateItems;
        }
    }
}