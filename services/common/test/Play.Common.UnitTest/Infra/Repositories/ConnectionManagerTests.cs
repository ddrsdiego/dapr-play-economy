namespace Play.Common.UnitTest.Infra.Repositories;

using System.Data;
using System.Data.Common;
using Application.Infra.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Polly;

public class ConnectionManagerTests
{
    private DbProviderFactory _providerFactory;
    private IAsyncPolicy _resiliencePolicy;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        _providerFactory = Substitute.For<DbProviderFactory>();
        _resiliencePolicy = Substitute.For<IAsyncPolicy>();
        _connectionString = "connectionString";
    }

    [Test]
    public async Task GetOpenConnectionAsync_ShouldReturnOpenConnection_WhenCalled()
    {
        // Arrange
        var connection = Substitute.For<DbConnection>();
        connection.State.Returns(ConnectionState.Open);
        _providerFactory.CreateConnection().Returns(connection);

        _resiliencePolicy.ExecuteAsync(Arg.Any<Func<Task<DbConnection>>>())
            .Returns(_ => _.Arg<Func<Task<DbConnection>>>().Invoke());

        var connectionManager = new ConnectionManager(_providerFactory, _connectionString, _resiliencePolicy);

        // Act
        var result = await connectionManager.GetOpenConnectionAsync();

        // Assert
        result.Should().Be(connection);
        await connectionManager.CloseAsync();
    }

    [Test]
    public async Task GetOpenConnectionAsync_ShouldOpenConnection_WhenConnectionIsNotOpen()
    {
        // Arrange
        var connection = Substitute.For<DbConnection>();
        connection.State.Returns(ConnectionState.Closed);
        _providerFactory.CreateConnection().Returns(connection);

        _resiliencePolicy.ExecuteAsync(Arg.Any<Func<Task<DbConnection>>>())
            .Returns(_ => _.Arg<Func<Task<DbConnection>>>().Invoke());

        var connectionManager = new ConnectionManager(_providerFactory, _connectionString, _resiliencePolicy);

        // Act
        var result = await connectionManager.GetOpenConnectionAsync();

        // Assert
        result.Should().Be(connection);
        await connectionManager.CloseAsync();
        await connection.Received().OpenAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetOpenConnectionAsync_ShouldReturnNewConnection_WhenConnectionIsNull()
    {
        // Arrange
        var connection1 = Substitute.For<DbConnection>();
        connection1.State.Returns(ConnectionState.Closed);

        var connection2 = Substitute.For<DbConnection>();
        connection2.State.Returns(ConnectionState.Closed);

        _providerFactory.CreateConnection().Returns(connection1, connection2);

        var connectionManager = new ConnectionManager(_providerFactory, _connectionString, null);

        // Act
        var result1 = await connectionManager.GetOpenConnectionAsync();
        await connectionManager.CloseAsync();
        var result2 = await connectionManager.GetOpenConnectionAsync();
        await connectionManager.CloseAsync();

        // Assert
        result1.Should().Be(connection1);
        result2.Should().Be(connection2);
        await connection1.DidNotReceive().OpenAsync(Arg.Any<CancellationToken>());
        await connection2.Received().OpenAsync(Arg.Any<CancellationToken>());
    }
}