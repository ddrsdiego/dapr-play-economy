namespace Play.Common.Application.Infra.Repositories.Dapr
{
    internal static class KeyFormatterHelper
    {
        public static string ConstructStateStoreKey(string entityName, string key) =>
            $"{entityName.ToLowerInvariant()}-{key}";
    }
}