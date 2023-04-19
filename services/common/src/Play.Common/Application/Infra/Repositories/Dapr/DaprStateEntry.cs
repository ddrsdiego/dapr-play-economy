namespace Play.Common.Application.Infra.Repositories.Dapr
{
    public abstract class DaprStateEntry : IDaprStateEntry
    {
        protected DaprStateEntry(string stateEntryKey)
        {
            StateEntryKey = stateEntryKey;
        }

        public string StateEntryKey { get; protected set; }
    }
}