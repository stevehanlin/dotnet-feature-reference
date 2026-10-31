namespace CustomTimeProvider.Api;

/// <summary>Open 09:00 to 17:00 local business time, Monday to Friday.</summary>
public sealed class StoreHours(TimeProvider clock)
{
    private static readonly TimeSpan Opens = TimeSpan.FromHours(9);
    private static readonly TimeSpan Closes = TimeSpan.FromHours(17);

    public DateTimeOffset LocalNow => clock.GetLocalNow();

    public bool IsOpen()
    {
        var now = LocalNow;
        return now.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
            && now.TimeOfDay >= Opens
            && now.TimeOfDay < Closes;
    }
}

/// <summary>Counts ticks of a timer created from the TimeProvider, so tests can advance time instead of sleeping.</summary>
public sealed class Heartbeat : IDisposable
{
    private readonly ITimer _timer;
    private int _beats;

    public Heartbeat(TimeProvider clock, TimeSpan interval) =>
        _timer = clock.CreateTimer(_ => Interlocked.Increment(ref _beats), state: null, dueTime: interval, period: interval);

    public int Beats => Volatile.Read(ref _beats);

    public void Dispose() => _timer.Dispose();
}
