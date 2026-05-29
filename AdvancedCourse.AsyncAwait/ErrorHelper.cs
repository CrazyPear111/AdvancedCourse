namespace AdvancedCourse.AsyncAwait;

public static class ErrorHelper
{
    public static Task<TResult[]> WhenAllOrError<TResult>(params Task<TResult>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (tasks.Length == 0)
            return Task.FromResult(Array.Empty<TResult>());

        var tcs = new TaskCompletionSource<TResult[]>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var results = new TResult[tasks.Length];
        int remaining = tasks.Length;

        for (int i = 0; i < tasks.Length; i++)
        {
            int index = i;
            Task<TResult> task = tasks[index];

            if (task == null)
            {
                tcs.TrySetException(
                    new ArgumentException("Tasks collection contains null task."));
                continue;
            }

            task.ContinueWith(completedTask =>
            {
                if (completedTask.IsFaulted)
                {
                    tcs.TrySetException(
                        completedTask.Exception!.InnerExceptions);
                    return;
                }

                if (completedTask.IsCanceled)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                results[index] = completedTask.Result;

                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    tcs.TrySetResult(results);
                }
            }, TaskScheduler.Default);
        }

        return tcs.Task;
    }
}
