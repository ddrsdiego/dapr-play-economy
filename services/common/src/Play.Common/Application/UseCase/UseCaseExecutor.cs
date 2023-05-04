namespace Play.Common.Application.UseCase;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public abstract class  UseCaseExecutor<TRequest> : IUseCaseExecutor<TRequest> where TRequest : UseCaseRequest
{
    protected UseCaseExecutor(ILogger logger)
    {
        Logger = logger;
    }

    protected ILogger Logger { get; }

    public async Task<Response> SendAsync(TRequest request, CancellationToken token = default)
    {
        Response response;
            
        Logger.LogInformation("");
            
        try
        {
            response = await ExecuteSendAsync(request, token);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "");
            throw;
        }
            
        Logger.LogInformation("");
            
        return response;
    }

    protected abstract Task<Response> ExecuteSendAsync(TRequest request, CancellationToken token = default);
}