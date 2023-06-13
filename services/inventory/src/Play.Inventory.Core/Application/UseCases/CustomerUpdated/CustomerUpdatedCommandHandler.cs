namespace Play.Inventory.Core.Application.UseCases.CustomerUpdated;

using Common.Application;
using Common.Application.Infra.Repositories.Dapr;
using Domain.AggregateModel.CustomerAggregate;
using Infra.Repositories.CustomerRepository;
using MediatR;
using Microsoft.AspNetCore.Http;

internal sealed class CustomerUpdatedCommandHandler : IRequestHandler<UpdateCustomerNameCommand,Response>
{
    private readonly IDaprStateEntryRepository<CustomerData> _customerDaprRepository;

    public CustomerUpdatedCommandHandler(IDaprStateEntryRepository<CustomerData> customerDaprRepository)
    {
        _customerDaprRepository = customerDaprRepository;
    }

    public async Task<Response> Handle(UpdateCustomerNameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // throw new Exception("Test");
            
            var customerResult = await _customerDaprRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
            if (customerResult.IsFailure)
                Response.Ok();
            
            var newCustomer = new Customer(request.CustomerId, request.Name,
                request.Email);
                
            await _customerDaprRepository.UpsertAsync(newCustomer.ToStateEntry(), cancellationToken);
            return Response.Ok(StatusCodes.Status204NoContent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response.Ok(StatusCodes.Status400BadRequest);
        }
    }
}