namespace Play.Inventory.Core.Application.UseCases.CustomerUpdated;

using Common.Application;
using MediatR;

public record struct CustomerUpdatedCommand(string CustomerId, string Name, string Email) : IRequest<Response>;