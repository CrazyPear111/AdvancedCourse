using AdvancedCourse.Tasks;
using AdvancedCourse.Threads;

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
await CustomTask.Delay(3000);
Console.WriteLine("After custom delay.");