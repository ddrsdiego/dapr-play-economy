namespace Play.Common.Application.UseCase
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IUseCaseExecutor<in TRequest>
        where TRequest : UseCaseRequest
    {
        Task<Response> SendAsync(TRequest request, CancellationToken token = default);
    }
}