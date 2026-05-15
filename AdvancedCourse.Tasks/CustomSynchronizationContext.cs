using System.Collections.Concurrent;

namespace AdvancedCourse.Tasks;

public class CustomSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly record struct WorkItem(
        SendOrPostCallback Callback,
        object State);

    private readonly BlockingCollection<WorkItem> _queue = [];
    private readonly Thread _thread;

    public CustomSynchronizationContext()
    {
        _thread = new Thread(ThreadLoop)
        {
            IsBackground = false,
            Name = "CustomSynchronizationContextThread"
        };

        _thread.Start();
    }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        _queue.Add(new WorkItem(callback, state));
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        if (Thread.CurrentThread.ManagedThreadId == _thread.ManagedThreadId)
        {
            callback(state);
            return;
        }

        using var done = new ManualResetEventSlim(false);

        Post(s =>
        {
            callback(s);
            done.Set();
        }, state);

        done.Wait();
    }

    private void ThreadLoop()
    {
        SetSynchronizationContext(this);

        foreach (var workItem in _queue.GetConsumingEnumerable())
        {
            workItem.Callback(workItem.State);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _queue.CompleteAdding();
        _thread.Join();
        _queue.Dispose();
    }
}
