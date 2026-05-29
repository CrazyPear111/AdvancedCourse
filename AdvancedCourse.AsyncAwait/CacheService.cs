namespace AdvancedCourse.AsyncAwait;

public class CacheService
{
    public static async Task Run()
    {
        var cache = 0;
        var tcs = new TaskCompletionSource(); // Добавить TaskCreationOptions.RunContinuationsAsynchronously, иначе может быть inline SetResult
        var cacheService = Task.Run(() =>
        {
            Console.WriteLine("Load init cache value from the Storage");
            Thread.Sleep(1000); // Task.Delay(1000)
            Console.WriteLine("Cache loaded");
            Volatile.Write(ref cache, 1000);
            tcs.SetResult();
            while (true)
            {
                Thread.Sleep(1000); // Task.Delay(1000)
                Console.WriteLine("Updating cache from storage");
                Volatile.Write(ref cache, cache + 1); // Interlocked.Increment(ref cache);
                cache++; // Зачем второй раз обновлять?
            }
        });

        var processingService = Task.Run(async () =>
        {
            Console.WriteLine("Processing service starting...");
            Console.WriteLine("Wating for cache loads...");
            await tcs.Task;
            while (true)
            {
                Thread.Sleep(1000); // Task.Delay(1000)
                Console.WriteLine($"Work with cache {Volatile.Read(ref cache)}");
            }
        });

        await Task.WhenAll(cacheService, processingService);
    }
}