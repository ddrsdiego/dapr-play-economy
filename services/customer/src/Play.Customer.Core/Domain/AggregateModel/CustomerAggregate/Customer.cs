namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate;

using System;
using Common.Domain.SeedWorks;
using CSharpFunctionalExtensions;
using Entity = Common.Domain.SeedWorks.Entity;

public sealed class Customer : Entity, IAggregateRoot
{
    private Customer()
        : this(string.Empty, string.Empty, string.Empty, string.Empty, default)
    {
    }

    public Customer(string document, string name, string email)
        : this(Guid.NewGuid().ToString().Split('-')[0], document, name, email, DateTimeOffset.UtcNow)
    {
        AddNotification(new NewCustomerCreated(Identification.Id, Name, Email.Value, CreatedAt));
    }

    internal Customer(string customerId, string document, string name, string electronicMail, DateTimeOffset createdAt)
        : base(customerId)
    {
        var emailResult =  Email.Create(electronicMail);
        if (emailResult.IsFailure)
            throw new ArgumentException(emailResult.Error, nameof(electronicMail));
        
        var documentResult = Document.Create(document);
        if (documentResult.IsFailure)
            throw new ArgumentException(documentResult.Error, nameof(document));
        
        Name = name;
        Identification = new CustomerIdentification(customerId, emailResult.Value.Value, document);
        Email = emailResult.Value;
        Document = documentResult.Value;
        CreatedAt = createdAt;
    }

    public static Customer Default => new();

    public bool IsValidCustomer => !string.IsNullOrEmpty(Identification.Value);

    public CustomerIdentification Identification { get; }
    public Document Document { get; }
    public string Name { get; private set; }
    public Email Email { get; }
    public DateTimeOffset CreatedAt { get; }

    public void UpdateName(string name)
    {
        Name = name;
        AddNotification(new CustomerNameUpdated(Identification.Id, Name));
    }
}

public readonly struct Document
{
    private Document(string document) => Value = document;

    public static Result<Document> Create(string document)
    {
        document = document.Trim().Replace(".", "").Replace("-", "");

        if (document.Length != 11)
            return Result.Failure<Document>("Document must be 11 characters long");

        for (var i = 0; i < 10; i++)
        {
            if (document == new string(Convert.ToChar(i.ToString()), 11))
                return Result.Failure<Document>("Document is invalid");
        }

        var multipliers1 = new[] {10, 9, 8, 7, 6, 5, 4, 3, 2};
        var multipliers2 = new[] {11, 10, 9, 8, 7, 6, 5, 4, 3, 2};

        var tempDocument = document.Substring(0, 9);
        var soma = 0;

        for (var i = 0; i < 9; i++)
            soma += int.Parse(tempDocument[i].ToString()) * multipliers1[i];

        var remainder = soma % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        var digit = remainder.ToString();
        tempDocument += digit;
        soma = 0;

        for (var i = 0; i < 10; i++)
        {
            soma += int.Parse(tempDocument[i].ToString()) * multipliers2[i];
        }

        remainder = soma % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        digit = digit + remainder.ToString();

        return document.EndsWith(digit)
            ? Result.Success(new Document(document))
            : Result.Failure<Document>("Document is invalid");
    }

    public string Value { get; }
}