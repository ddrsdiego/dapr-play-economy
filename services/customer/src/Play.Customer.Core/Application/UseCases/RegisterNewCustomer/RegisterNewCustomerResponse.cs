namespace Play.Customer.Core.Application.UseCases.RegisterNewCustomer
{
    using System;
    using System.Text.Json.Serialization;

    public readonly struct RegisterNewCustomerResponse
    {
        [JsonConstructor]
        public RegisterNewCustomerResponse(string customerId, string name, string email, DateTimeOffset createdAt)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
            CreatedAt = createdAt;
        }
        
        public string CustomerId { get; }
        public string Name { get; }
        public string Email { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}