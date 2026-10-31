namespace CustomTimeProvider.Api;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers a <see cref="TimeProvider"/> whose local time is the given business zone.</summary>
    public static IServiceCollection AddBusinessClock(this IServiceCollection services, string timeZoneId)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        services.AddSingleton<TimeProvider>(new ZonedTimeProvider(TimeProvider.System, zone));
        return services;
    }

    public static IServiceCollection AddStoreServices(this IServiceCollection services)
    {
        services.AddSingleton<StoreHours>();
        services.AddSingleton(sp => new Heartbeat(sp.GetRequiredService<TimeProvider>(), TimeSpan.FromSeconds(30)));
        return services;
    }
}
