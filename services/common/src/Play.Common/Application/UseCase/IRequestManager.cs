namespace Play.Common.Application.UseCase;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class RequestManagerEntry
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
}

public interface IRequestManager
{
    Task<bool> ExistAsync<TCommand>(string id, out TCommand command);

    Task CreateRequestForCommandAsync<T>(string id);
}

public sealed class RequestManagerMemoryCache : IRequestManager
{
    private readonly IMemoryCache _memoryCache;

    public RequestManagerMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<bool> ExistAsync<TCommand>(string id, out TCommand command)
    {
        var exists = _memoryCache.TryGetValue(id, out command);
        return Task.FromResult(exists);
    }

    public Task CreateRequestForCommandAsync<T>(string id)
    {
        if (_memoryCache.TryGetValue(id, out RequestManagerEntry entry))
            throw new Exception($"Request with {id} already exists");

        entry = new RequestManagerEntry
        {
            Id = id,
            Name = typeof(T).Name,
            Time = DateTime.UtcNow
        };

        _memoryCache.Set(entry.Id, entry);

        return Task.CompletedTask;
    }
}