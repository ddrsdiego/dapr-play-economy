namespace Play.Common.Application.Infra.UoW;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

public readonly struct UnitOfWorkProcess
{
    private readonly Stopwatch _stopwatch;

    internal UnitOfWorkProcess(string unitOfWorkContextId, string workId, Func<Task> task)
    {
        Method = task;
        UnitOfWorkContextId = unitOfWorkContextId;
        WorkId = workId;
        _stopwatch = Stopwatch.StartNew();
    }

    public readonly string UnitOfWorkContextId;
    public readonly string WorkId;
    public readonly Func<Task> Method;
    internal long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    internal void StopWatch() => _stopwatch.Stop();
}