namespace Play.Common.Application.Infra.Repositories;

public interface ITransactionManagerFactory
{
    ITransactionManager CreateTransactionManager();
}

public sealed class TransactionManagerFactory : ITransactionManagerFactory
{
    public ITransactionManager CreateTransactionManager() => new TransactionManager();
}