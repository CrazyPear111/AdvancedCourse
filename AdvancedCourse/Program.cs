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
