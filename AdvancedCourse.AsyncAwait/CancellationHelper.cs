namespace AdvancedCourse.AsyncAwait;

public static class CancellationHelper
{
    public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken ct)
    {
        if (task.IsCompleted)
            return await task;

        var tcs = new TaskCompletionSource<object>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        using (ct.Register(
                   s => ((TaskCompletionSource<object>)s!).TrySetResult(null!),
                   tcs))
        {
            var completedTask = await Task.WhenAny(task, tcs.Task);
            if (completedTask == tcs.Task)
            {
                throw new OperationCanceledException(ct);
            }
        }

        return await task;
    }
}
