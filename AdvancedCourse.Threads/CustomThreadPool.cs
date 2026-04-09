using System.Collections.Concurrent;

namespace AdvancedCourse.Threads;

public static class CustomThreadPool
{
    private const int ThreadCount = 10;
    private static readonly List<Thread> _threads = [];
    private static readonly ConcurrentQueue<UserWorkItem> _queue = [];
    private static readonly AutoResetEvent _workItemAdded = new(false);

    static CustomThreadPool()
    {
        for (int i = 0; i < ThreadCount; i++)
        {
            var thread = new Thread(Process);
            thread.Start();
            _threads.Add(thread);
        }
    }

    public static bool QueueUserWorkItem(Action<object?> callback, object? state = null)
    {
        _queue.Enqueue(new(callback, state));
        _workItemAdded.Set();
        return true;
    }

    private static void Process()
    {
        while (true)
        {
            if (_queue.TryDequeue(out UserWorkItem? workItem))
            {
                var (callback, state) = workItem;
                callback(state);
            }
            else
            {
                _workItemAdded.WaitOne();
            }
        }
    }

    record UserWorkItem(Action<object?> Callback, object? State);
}
