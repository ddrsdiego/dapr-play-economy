namespace Play.Inventory.Service.Subscribers.Messages;

using Core.Application.UseCases.CustomerUpdated;

public struct CustomerNameUpdated
{
    public string CustomerId { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }

    public UpdateCustomerNameCommand ToCommand() => new(CustomerId, Name, Email);
}