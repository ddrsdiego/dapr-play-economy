namespace Play.Common.Application.Infra.Repositories.Dapr;

public interface IDaprStateEntry
{
    string StateEntryKey { get; }
}