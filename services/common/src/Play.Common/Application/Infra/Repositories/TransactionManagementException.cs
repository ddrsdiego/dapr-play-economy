namespace Play.Common.Application.Infra.Repositories;

using System;

[Serializable]
public class TransactionManagementException : Exception
{
    public TransactionManagementException()
        : base()
    {
    }

    public TransactionManagementException(string message)
        : base(message)
    {
    }

    public TransactionManagementException(string message, Exception innerException) : base(message, innerException)
    {
    }
}