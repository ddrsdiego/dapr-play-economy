namespace Play.Common.Application.Messaging.OutBox;

using Infra.Repositories;

public interface IOutBoxMessageHouseKeepingRepository
{
    
}

public sealed class OutBoxMessageHouseKeepingRepository : IOutBoxMessageHouseKeepingRepository
{
    private readonly IConnectionManager _connectionManager;

    public OutBoxMessageHouseKeepingRepository(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }
}