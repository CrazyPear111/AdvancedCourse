using AdvancedCourse.AsyncAwait;
using AdvancedCourse.Tasks;
using AdvancedCourse.Threads;
using System.Diagnostics;

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

//-------------- 15. WithCancellation helper ---------------
Console.WriteLine("-------------- WithCancellation helper ---------------");

var cts = new CancellationTokenSource(800);
var task = Task.Run(async () =>
{
    await Task.Delay(1000);
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

//-------------- 16. Error helper ---------------
Console.WriteLine("-------------- Error helper ---------------");

var t1 = Task.Run(async () =>
{
    await Task.Delay(3000);
    return 1;
});

var t2 = Task.Run<int>(async () =>
{
    await Task.Delay(500);
    throw new InvalidOperationException("Failed");
});

var t3 = Task.Run(async () =>
{
    await Task.Delay(5000);
    return 3;
});

try
{
    var result = await ErrorHelper.WhenAllOrError(t1, t2, t3);
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
}


//-------------- 18. Cache service ---------------
Console.WriteLine("-------------- Cache service ---------------");

//await CacheService.Run();


//-------------- 19. Synchronization context tests ---------------
Console.WriteLine("-------------- Synchronization context tests ---------------");

SynchronizationContext.SetSynchronizationContext(new CustomSynchronizationContext());
Console.WriteLine($"App start, Thread:{Environment.CurrentManagedThreadId}");
var sct1 = Task.Delay(1000);
await sct1;
Console.WriteLine($"Context - {SynchronizationContext.Current is not null}"); // true (контекст переключился)
Console.WriteLine($"After await:{Environment.CurrentManagedThreadId}"); // другой поток (поток контекста)

Console.WriteLine("------------------------------------------");

SynchronizationContext.SetSynchronizationContext(new CustomSynchronizationContext());
Console.WriteLine($"App start, Thread:{Environment.CurrentManagedThreadId}");
var sct2 = Task.Delay(1000).ConfigureAwait(false);
await sct2;
Console.WriteLine($"Context - {SynchronizationContext.Current is not null}"); // false, context НЕ переключился
Console.WriteLine($"After await:{Environment.CurrentManagedThreadId}");// другой поток (поток пула)

Console.WriteLine("------------------------------------------");

SynchronizationContext.SetSynchronizationContext(new CustomSynchronizationContext());
Console.WriteLine($"App start, Thread:{Environment.CurrentManagedThreadId}");
var sct3 = Task.Delay(1000).ConfigureAwait(false);
Thread.Sleep(2000);
await sct3; // task уже завершена, синхронное выполнение
Console.WriteLine($"Context - {SynchronizationContext.Current is not null}"); // true, context НЕ переключился
Console.WriteLine($"After await:{Environment.CurrentManagedThreadId}"); // тот же поток


//-------------- 20. WhenAny / WhenAll continuations ---------------
Console.WriteLine("-------------- WhenAny / WhenAll continuations ---------------");

var tcs1 = new TaskCompletionSource();
var twa1 = new Thread(() =>
{
    Console.WriteLine($"T1 - {Environment.CurrentManagedThreadId}");
    Thread.Sleep(1000);
    tcs1.SetResult();   // inline
    Console.WriteLine($"Finish T1 - {Environment.CurrentManagedThreadId}");
});

var tcs2 = new TaskCompletionSource();
var twa2 = new Thread(() =>
{
    Console.WriteLine($"T2 - {Environment.CurrentManagedThreadId}");
    Thread.Sleep(2000);
    tcs2.SetResult();   // inline
    Console.WriteLine($"Finish T2 - {Environment.CurrentManagedThreadId}");
});

twa1.Start();
twa2.Start();

await Task.WhenAny(tcs1.Task, tcs2.Task); // продолжится на потоке T1, т.к. внутри WhenAny вызовется continuation для первой завершившейся задачи
//await Task.WhenAll(tcs1.Task, tcs2.Task); // продолжится на потоке T2, т.к. внутри WhenAll вызовется continuation для последней завершившейся задачи

Console.WriteLine($"After await  - {Environment.CurrentManagedThreadId}");


//-------------- 21. What will be the result of the program's execution? ---------------

var l = new List<Task>(); // List - не потокобезопасный
_ = Task.Run(() =>
{
    while (true)
    {
        l.Add(Task.Delay(10_000)); // добавляет элементы в List
    }
});
Thread.Sleep(1000);
await Task.WhenAny(l); // перечисляет List, но он одновременно меняется, будет InvalidOperationException
// или память закончится


//-------------- 22. What will be the result of the program's execution? ---------------
async Task fail() => throw new Exception();
async Task wait() => await Task.Delay(TimeSpan.FromSeconds(2));
async Task waitAndFail()
{
    await Task.Delay(
        TimeSpan.FromSeconds(1));
    throw new Exception();
}

var stopwatch = Stopwatch.StartNew();
try
{
    await Task.WhenAll(fail(), wait(), wait(), waitAndFail()); // подождет все task'и и только потом бросит исключение
}
catch { }

stopwatch.Stop();
var elapsedSeconds = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds;
Console.WriteLine(elapsedSeconds >= 0); // true
Console.WriteLine(elapsedSeconds >= 1); // true
Console.WriteLine(elapsedSeconds >= 2); // true


//-------------- 23. What will be the result of the program's execution? ---------------
var tcs = new TaskCompletionSource();
using var token = new CancellationTokenSource();
token.Token.Register(() =>
{
    tcs.SetResult(); // inline (бесконечный цикл)
    Console.WriteLine("Token has been cancelled."); // никогда не выведется в консоль
});

var t = Task.Run(async () =>
{
    await tcs.Task;
    while (true)
    {
        Thread.Sleep(1000); //some long work;
        Console.WriteLine("Job done");
    }
});

await Task.Delay(1000);
token.Cancel();
await t;
// выведутся бесконечные Job done
