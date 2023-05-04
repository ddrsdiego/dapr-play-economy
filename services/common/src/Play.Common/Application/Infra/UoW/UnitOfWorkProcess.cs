namespace Play.Common.Application.Infra.UoW;

using System;
using System.Threading.Tasks;

public readonly struct UnitOfWorkProcess
{
    public UnitOfWorkProcess(string unitOfWorkContextId, Func<Task> task)
    {
        Method = task;
        UnitOfWorkContextId = unitOfWorkContextId;
    }

    public string UnitOfWorkContextId { get; }

    public Func<Task> Method { get; }
}