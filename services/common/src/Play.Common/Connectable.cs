﻿namespace LogCo.Delivery.GestaoEntregas.RouterAdapter.CrossCutting.Commons;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Connectable<T>
    where T : class
{
    private readonly Dictionary<long, T> _connections;
    private T[] _connected;
    private long _nextId;

    public Connectable()
    {
        _connections = new Dictionary<long, T>();
        _connected = Array.Empty<T>();
    }

    /// <summary>
    /// The number of connections
    /// </summary>
    public int Count => _connected.Length;

    /// <summary>
    /// Connect a connectable type
    /// </summary>
    /// <param name="connection">The connection to add</param>
    /// <returns>The connection handle</returns>
    public IConnectHandle Connect(T connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        var id = Interlocked.Increment(ref _nextId);

        lock (_connections)
        {
            _connections.Add(id, connection);
            _connected = _connections.Values.ToArray();
        }

        return new Handle(id, this);
    }

    /// <summary>
    /// Enumerate the connections invoking the callback for each connection
    /// </summary>
    /// <param name="callback">The callback</param>
    /// <returns>An awaitable Task for the operation</returns>
    public Task ForEachAsync(Func<T, Task> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        T[] connected;
        lock (_connections)
            connected = _connected;

        switch (connected.Length)
        {
            case 0:
                return Task.CompletedTask;
            case 1:
                return callback(connected[0]);
        }

        var outputTasks = new Task[connected.Length];

        int index;
        for (index = 0; index < connected.Length; index++)
            outputTasks[index] = callback(connected[index]);

        for (index = 0; index < outputTasks.Length; index++)
        {
            if (outputTasks[index].Status != TaskStatus.RanToCompletion)
                break;
        }

        return index == outputTasks.Length ? Task.CompletedTask : Task.WhenAll(outputTasks);
    }

    public void ForEach(Action<T> callback)
    {
        T[] connected;
        lock (_connections)
            connected = _connected;

        switch (connected.Length)
        {
            case 0:
                break;
            case 1:
                callback(connected[0]);
                break;
            default:
            {
                for (var i = 0; i < connected.Length; i++)
                    callback(connected[i]);
                break;
            }
        }
    }

    public bool All(Func<T, bool> callback)
    {
        T[] connected;
        lock (_connections)
            connected = _connected;

        if (connected.Length == 0)
            return true;

        if (connected.Length == 1)
            return callback(connected[0]);

        for (var i = 0; i < connected.Length; i++)
        {
            if (callback(connected[i]) == false)
                return false;
        }

        return true;
    }

    void Disconnect(long id)
    {
        lock (_connections)
        {
            _connections.Remove(id);
            _connected = _connections.Values.ToArray();
        }
    }


    public class Handle :
        IConnectHandle
    {
        readonly Connectable<T> _connectable;
        readonly long _id;

        public Handle(long id, Connectable<T> connectable)
        {
            _id = id;
            _connectable = connectable;
        }

        public void Disconnect()
        {
            _connectable.Disconnect(_id);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}