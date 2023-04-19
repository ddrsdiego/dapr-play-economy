namespace Play.Common.Application.Infra.Repositories.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using CSharpFunctionalExtensions;
    using global::Dapr.Client;

    internal static class BulkStateItemExtensions
    {
        public static bool TryDeserializeValue<TEntry>(this BulkStateItem bulkStateItem, string originalKey,
            IDictionary<string, Result<TEntry>> results, out TEntry entity)
        {
            entity = default;

            try
            {
                entity = JsonSerializer.Deserialize<TEntry>(bulkStateItem.Value);
            }
            catch (Exception e)
            {
                var failureMessage =
                    $"Failure to deserialize for id: {originalKey}. Error Message: {e.Message}";

                results.Add(originalKey, Result.Failure<TEntry>(failureMessage));
                return true;
            }

            return false;
        }

        public static bool CheckIfValueIsNotEmpty<TEntry>(this BulkStateItem bulkStateItem,
            IDictionary<string, Result<TEntry>> results, string originalKey)
        {
            if (!string.IsNullOrEmpty(bulkStateItem.Value))
                return false;

            results.Add(originalKey, Result.Failure<TEntry>($"Id: {originalKey} not found."));
            return true;
        }
    }
}