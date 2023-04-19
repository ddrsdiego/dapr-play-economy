namespace Play.Customer.Core.Application.UseCases.GetCustomerById
{
    using System;

    public sealed record GetCustomerByIdResponse(string CustomerId, string Name, string Email, DateTimeOffset CreatedAt);
}