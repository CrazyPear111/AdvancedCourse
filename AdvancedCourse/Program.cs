using AdvancedCourse.Tasks;
using AdvancedCourse.Threads;
using AdvancedCourse.AsyncAwait;

//-------------- ThreadCoordinator ---------------
Console.WriteLine("-------------- ThreadCoordinator ---------------");

var elements = Enumerable.Range(1, 20);
var coordinator = new ThreadCoordinator();

var results = coordinator.Compute(elements);
results.ForEach(Console.WriteLine);


//-------------- ThreadPool ---------------
Console.WriteLine("-------------- ThreadPool ---------------");

CustomThreadPool.QueueUserWorkItem(
    (obj) => Console.WriteLine(obj),
    5);

await Task.Delay(100);

//-------------- Task ---------------
Console.WriteLine("-------------- Task ---------------");

var taskLoop = new TaskLoop
{
    A = () => Console.WriteLine($"After delay {Thread.CurrentThread.ManagedThreadId}"),
    Max = 5,
};

Console.WriteLine($"Hello world {Thread.CurrentThread.ManagedThreadId}");
taskLoop.Run();
taskLoop.Task.Wait();

Console.WriteLine("The End.");

//Custom delay
await CustomTask.Delay(1000);
Console.WriteLine("After custom delay.");

//-------------- Synchronization context ---------------
Console.WriteLine("-------------- Synchronization context ---------------");

using var context = new CustomSynchronizationContext();
SynchronizationContext.SetSynchronizationContext(context);

Console.WriteLine($"Before delay: {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}");
await Task.Delay(1000);
Console.WriteLine($"After delay: {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}");
await Task.Delay(1000);
Console.WriteLine($"After second delay: {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}");

SynchronizationContext.SetSynchronizationContext(default);

//-------------- WithCancellation helper ---------------
Console.WriteLine("-------------- WithCancellation helper ---------------");

var cts = new CancellationTokenSource(1000);
var task = Task.Run(async () =>
{
    await Task.Delay(10000);
    Console.WriteLine("Real task finished.");
    return 42;
});

try
{
    await task.WithCancellation(cts.Token);
    // new way
    //await task.WaitAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine(nameof(OperationCanceledException));
}
