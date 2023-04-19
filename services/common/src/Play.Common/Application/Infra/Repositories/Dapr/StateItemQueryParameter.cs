namespace Play.Common.Application.Infra.Repositories.Dapr
{
    internal readonly struct StateItemQueryParameter<TEntry>
    {
        // public StateItemQueryParameter(string originalKey)
        //     :this(KeyFormatterHelper.ConstructStateStoreKey(typeof(TEntry).Name, originalKey), originalKey)
        // {
        //     OriginalKey = originalKey;
        //     FormattedKey = KeyFormatterHelper.ConstructStateStoreKey(typeof(TEntry).Name, OriginalKey);
        // }

        public StateItemQueryParameter(string stateEntryName, string originalKey)
        {
            OriginalKey = originalKey;
            FormattedKey = KeyFormatterHelper.ConstructStateStoreKey(stateEntryName, OriginalKey);
        }
        
        public readonly string OriginalKey;
        public readonly string FormattedKey;
    }
}