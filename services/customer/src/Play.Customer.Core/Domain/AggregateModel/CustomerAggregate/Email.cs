namespace Play.Customer.Core.Domain.AggregateModel.CustomerAggregate;

using CSharpFunctionalExtensions;

public readonly struct Email
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string electronicMail)
    {
        if (string.IsNullOrEmpty(electronicMail))
            return Result.Failure<Email>("Email should not be empty");

        var atSignIndex = electronicMail.IndexOf('@');
        if (atSignIndex <= 0 || atSignIndex == electronicMail.Length - 1)
            return Result.Failure<Email>("Email is invalid");

        var dotIndex = electronicMail.LastIndexOf('.');
        if (dotIndex <= atSignIndex + 1 || dotIndex == electronicMail.Length - 1)
            return Result.Failure<Email>("Email is invalid");

        var localPart = electronicMail.Substring(0, atSignIndex);
        var domainPart = electronicMail.Substring(atSignIndex + 1);

        if (ContainsInvalidCharacters(localPart) || ContainsInvalidCharacters(domainPart))
            return Result.Failure<Email>("Email is invalid");

        if (localPart.Contains(".."))
            return Result.Failure<Email>("Email is invalid");

        var domainSegments = domainPart.Split('.');

        foreach (var segment in domainSegments)
        {
            if (string.IsNullOrEmpty(segment) || segment.StartsWith("-") || segment.EndsWith("-"))
                return Result.Failure<Email>("Email is invalid");
        }

        return Result.Success(new Email(electronicMail));
    }
        
    private static bool ContainsInvalidCharacters(string input)
    {
        foreach (var c in input)
        {
            if (c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' || c == '*' ||
                c == '+' || c == '-' || c == '/' || c == '=' || c == '?' || c == '^' || c == '_' ||
                c == '`' || c == '{' || c == '|' || c == '}' || c == '~' || char.IsLetterOrDigit(c) || c == '.')
            {
                continue;
            }
            return true;
        }

        return false;
    }
}