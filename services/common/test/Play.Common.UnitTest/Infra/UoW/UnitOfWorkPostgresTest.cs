namespace Play.Common.UnitTest.Infra.UoW;

using Application.Infra.Repositories;
using Application.Infra.UoW;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

[TestFixture]
public class UnitOfWorkPostgresTest
{
    private IConnectionManager _connectionManager;
    private ITransactionManager _transactionManager;
    private IUnitOfWork _unitOfWork;

    [SetUp]
    public void SetUp()
    {
        _connectionManager = Substitute.For<IConnectionManager>();
        _transactionManager = Substitute.For<ITransactionManager>();
    }

    [Test]
    public async Task SaveChangesAsync_WithoutBeginTransaction_ShouldThrowInvalidOperationException()
    {
        _unitOfWork = UnitOfWorkPostgres.Create(_connectionManager, new CancellationToken(false));

        var act = async () => await _unitOfWork.SaveChangesAsync();
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("There is no transaction in progress.");
    }
    
    [Test]
    public async Task SaveChangesAsync_ShouldCallCommitAsyncOnTransactionManager()
    {
        _connectionManager.ConnectionString.Returns(x => "connection string");
        
        _connectionManager.TransactionManager.Returns(_transactionManager);
        _unitOfWork = UnitOfWorkPostgres.Create(_connectionManager, new CancellationToken(false));
        
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.ConnectionManager.TransactionManager.Received().CommitAsync(Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task BeginTransactionAsync_ShouldCallBeginTransactionAsyncOnConnectionManager()
    {
        _connectionManager.ConnectionString.Returns(x => "connection string");
        
        _connectionManager.TransactionManager.Returns(_transactionManager);
        _unitOfWork = UnitOfWorkPostgres.Create(_connectionManager, new CancellationToken(false));

        await _unitOfWork.BeginTransactionAsync();
        await _connectionManager.Received().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }
}