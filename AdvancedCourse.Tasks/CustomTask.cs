namespace AdvancedCourse.Tasks;

public static class CustomTask
{
    public static Task Delay(int millisecondsDelay)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsDelay, 0);

        if (millisecondsDelay == 0)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        Timer timer = null!;

        timer = new Timer(
            _ =>
            {
                timer.Dispose();
                tcs.SetResult(true);
            }, 
            null, 
            millisecondsDelay, 
            Timeout.Infinite);

        return tcs.Task;
    }

    public static Task Delay(TimeSpan delay) => Delay((int)delay.TotalMilliseconds);
}
