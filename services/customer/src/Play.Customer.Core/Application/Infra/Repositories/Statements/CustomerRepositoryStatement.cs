namespace Play.Customer.Core.Application.Infra.Repositories.Statements;

public abstract class CustomerRepositoryStatement
{
    private const string SelectDefault =
        "SELECT \"CustomerId\", \"Document\", \"Email\", \"Name\", \"CreatedAt\" FROM public.customers";

    public const string SaveAsync =
        "INSERT INTO public.customers(\"CustomerId\", \"Document\", \"Email\", \"Name\", \"CreatedAt\") VALUES (@CustomerId, @Document, @Email, @Name, @CreatedAt)";

    /// <summary>
    /// Parameters: CustomerId, Name
    /// </summary>
    public const string UpdateAsync =
        "UPDATE public.customers SET  \"Name\" = @Name WHERE ( \"CustomerId\" = @CustomerId )";
        
    /// <summary>
    /// Parameter: CustomerId
    /// </summary>
    public const string GetByIdAsync = $"{SelectDefault} where \"CustomerId\" = @CustomerId";

    /// <summary>
    /// Parameter: Document
    /// </summary>
    public const string GetByDocumentAsync = $"{SelectDefault} where \"Document\" = @Document";

    /// <summary>
    /// Parameter: Email
    /// </summary>
    public const string GetByEmailAsync = $"{SelectDefault} where \"Email\" = @Email";
}