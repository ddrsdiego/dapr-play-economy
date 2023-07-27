namespace Play.Common.Application.Messaging.InBox;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Infra.UoW;
using Observers.ProcessorExecute;
using Observers.ProcessorExecute.Observables;

/// <summary>
/// Defines a service for processing incoming messages ("InBox") asynchronously.
/// This interface is part of an Observer pattern, through its implementation of the IInBoxMessageProcessorObserverConnector interface.
/// </summary>
public interface IInBoxMessageProcessor : IInBoxMessageProcessorObserverConnector, IAsyncDisposable
{
    /// <summary>
    /// Asynchronously retrieves pending incoming messages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An asynchronous stream of pending InBoxMessage objects.</returns>
    IAsyncEnumerable<InBoxMessage> GetMessagesPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves pending incoming messages that are marked as priority.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An asynchronous stream of priority pending InBoxMessage objects.</returns>
    IAsyncEnumerable<InBoxMessage> GetPendingMessagesByPriorityAsync(CancellationToken cancellationToken = default);

    ValueTask EnqueueMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a provided asynchronous task on an incoming message.
    /// </summary>
    /// <typeparam name="TEvent">The type of the input to the task function.</typeparam>
    /// <param name="inBoxMessage">The incoming message on which to execute the task.</param>
    /// <param name="method">An asynchronous task function that takes an input of type TEvent.</param>
    /// <param name="stoppingToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task ExecuteAsync<TEvent>(InBoxMessage inBoxMessage, Func<InBoxMessage, TEvent, Task> method, CancellationToken stoppingToken = default);
}

internal sealed class InBoxMessageProcessor : IInBoxMessageProcessor
{
    private readonly IInBoxMessageReceiver _inBoxMessageReceiver;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IInBoxMessageStorage _inBoxMessageStorage;
    private readonly InBoxMessageProcessorObservable _inBoxMessageProcessorObservable;

    public InBoxMessageProcessor(IUnitOfWorkFactory unitOfWorkFactory, IInBoxMessageStorage inBoxMessageStorage, IInBoxMessageReceiver inBoxMessageReceiver)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _inBoxMessageStorage = inBoxMessageStorage;
        _inBoxMessageReceiver = inBoxMessageReceiver;
        _inBoxMessageProcessorObservable = new InBoxMessageProcessorObservable();
    }

    public IConnectHandle ConnectInBoxMessageProcessorExecuteObserver(IInBoxMessageProcessorObserver observer)
    {
        return _inBoxMessageProcessorObservable.Connect(observer);
    }

    public IAsyncEnumerable<InBoxMessage> GetMessagesPendingAsync(CancellationToken cancellationToken = default)
    {
        return _inBoxMessageStorage.GetPendingMessagesAsync(cancellationToken);
    }

    public async IAsyncEnumerable<InBoxMessage> GetPendingMessagesByPriorityAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var inboxMessage in _inBoxMessageStorage.GetPendingMessagesByPriorityAsync(cancellationToken))
        {
            yield return inboxMessage;
        }
    }

    public ValueTask EnqueueMessageAsync(InBoxMessage inBoxMessage, CancellationToken cancellationToken = default)
    {
        return _inBoxMessageReceiver.EnqueueMessageAsync(inBoxMessage, cancellationToken);
    }

    public async Task ExecuteAsync<TEnvent>(InBoxMessage inBoxMessage, Func<InBoxMessage, TEnvent, Task> method, CancellationToken stoppingToken = default)
    {
        try
        {
            await _inBoxMessageProcessorObservable.OnPreProcess(inBoxMessage);

            var resultEvent = inBoxMessage.TryParse<TEnvent>();
            if (resultEvent.IsFailure)
                throw new DeserializeInBoxMessagePayloadException(inBoxMessage, resultEvent.Error);

            await using var uow = await _unitOfWorkFactory.CreateAsync();

            await uow.AddToContextAsync(async () => await method(inBoxMessage, resultEvent.Value).ConfigureAwait(false));
            await uow.AddToContextAsync(async () => await _inBoxMessageStorage.CommitMessageAsync(inBoxMessage, stoppingToken));

            await uow.SaveChangesAsync(stoppingToken);

            await _inBoxMessageProcessorObservable.OnPostProcess(inBoxMessage);
        }
        catch (Exception e)
        {
            await _inBoxMessageProcessorObservable.OnProcessError(inBoxMessage, e);

            await using var uow = await _unitOfWorkFactory.CreateAsync();

            await uow.AddToContextAsync(async () => await _inBoxMessageStorage.IncrementRetryNumberAttemptsAsync(inBoxMessage, e.Message, stoppingToken));

            await uow.SaveChangesAsync(stoppingToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _inBoxMessageReceiver.DisposeAsync();
    }
}