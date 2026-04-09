namespace AdvancedCourse.Threads;

public class ThreadCoordinator
{
    private readonly List<int> _results = [];
    private readonly Lock _locker = new();

    public List<int> Compute(IEnumerable<int> elements)
    {
        List<Thread> threads = [];
        foreach (var element in elements)
        {
            var thread = new Thread(MultiplyByTwo);
            thread.Start(element);
            threads.Add(thread);
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return _results;
    }

    private void MultiplyByTwo(object? state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state is int value)
        {
            lock (_locker)
            {
                _results.Add(value * 2);
            }
        }
    }
}
