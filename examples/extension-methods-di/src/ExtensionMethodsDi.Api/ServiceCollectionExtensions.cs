using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExtensionMethodsDi.Api;

public static class ServiceCollectionExtensions
{
    // 1. Classic form: `this` on the first parameter. Works on every C# version.
    public static IServiceCollection AddClock(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);   // TryAdd: tests can register a fake first
        return services;
    }

    // 2. C# 14 extension block: the receiver is declared once, members read like normal methods.
    extension(IServiceCollection services)
    {
        // Options-style overload: callers tweak defaults in a lambda.
        public IServiceCollection AddGreeter(Action<GreeterOptions>? configure = null)
        {
            var builder = services.AddOptions<GreeterOptions>();
            if (configure is not null)
                builder.Configure(configure);

            services.TryAddSingleton<IGreeter, Greeter>();
            return services;
        }

        // Config-bound overload: same feature, driven by appsettings instead.
        public IServiceCollection AddAuditLog(IConfiguration configuration)
        {
            services.AddOptions<AuditLogOptions>().Bind(configuration.GetSection("AuditLog"));
            services.TryAddSingleton<IAuditLog, AuditLog>();
            return services;
        }

        // 3. Extension *property*: not possible with classic extension methods.
        public bool HasGreeter => services.Any(d => d.ServiceType == typeof(IGreeter));
    }
}
