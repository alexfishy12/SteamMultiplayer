using System;
using System.Collections.Concurrent;

public static class MainThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> _actions = new();

    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "Action cannot be null.");
        }
        _actions.Enqueue(action);
    }

    // Call this regularly from your main loop
    public static void ExecutePending()
    {
        while (_actions.TryDequeue(out var action))
        {
            action();
        }
    }
}