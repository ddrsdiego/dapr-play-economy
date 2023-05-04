namespace Play.Common.Application.UseCase;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

public class IdentifiedCommand<T> : UseCaseRequest, IRequest<Response> where T : UseCaseRequest
{
    public IdentifiedCommand(T command, string id) : base(id) => Command = command;
    public T Command { get; }
}

public sealed class IdentifiedCommandHandler<T> : IRequestHandler<IdentifiedCommand<T>, Response>
    where T : UseCaseRequest
{
    private readonly IMediator _mediator;
    private readonly IRequestManager _requestManager;
    private readonly ILogger<IdentifiedCommandHandler<T>> _logger;

    public async Task<Response> Handle(IdentifiedCommand<T> request, CancellationToken cancellationToken)
    {
        var alreadyExists = await _requestManager.ExistAsync<T>(request.RequestId, out var command);
        if (alreadyExists)
        {
        }

        command = request.Command;

        var rsp = await _mediator.Send(command, cancellationToken);

        return (Response) rsp;
    }
}