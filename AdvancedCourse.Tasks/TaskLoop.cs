namespace AdvancedCourse.Tasks;

public class TaskLoop
{
    private readonly TimeSpan _waitInterval = TimeSpan.FromSeconds(1);

    public required Action A { get; init; }

    public required int Max { get; init; }

    public Task Task { get; private set; } = Task.CompletedTask;

    public void Run()
    {
        Task = RunIteration();
    }

    private Task RunIteration(int current = 0)
    {
        if (current >= Max)
            return Task.CompletedTask;

        A();

        return Task
            .Delay(_waitInterval)
            .ContinueWith(_ => RunIteration(current + 1))
            .Unwrap();
    }
}
