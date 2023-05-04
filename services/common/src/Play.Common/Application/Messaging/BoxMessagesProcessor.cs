namespace Play.Common.Application.Messaging;

using System;
using Dapr.Client;
using Infra.UoW;

public abstract class BoxMessagesProcessor
{
    protected BoxMessagesProcessor(BoxMessagesProcessorConfig config, IUnitOfWorkFactory unitOfWorkFactory, DaprClient daprClient)
    {
        Config = config;
        UnitOfWorkFactory = unitOfWorkFactory;
        DaprClient = daprClient;
        LockOwner = Guid.NewGuid().ToString();
    }

    protected readonly string LockOwner;
    protected readonly DaprClient DaprClient;
    protected readonly IUnitOfWorkFactory UnitOfWorkFactory;
    protected readonly BoxMessagesProcessorConfig Config;
}