namespace Play.Common.Application.Infra.Repositories.Dapr
{
    internal readonly struct StateItemQueryParameter<TEntry>
    {
        public StateItemQueryParameter(string stateEntryName, string originalKey)
        {
            OriginalKey = originalKey;
            FormattedKey = KeyFormatterHelper.ConstructStateStoreKey(stateEntryName, OriginalKey);
        }

        public readonly string OriginalKey;
        public readonly string FormattedKey;
    }
}