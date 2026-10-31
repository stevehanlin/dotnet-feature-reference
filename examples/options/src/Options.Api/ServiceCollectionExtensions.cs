using Microsoft.Extensions.Options;

namespace Options.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind + validate eagerly: a bad config fails at startup, not on first request.
        services.AddOptions<GreetingOptions>()
            .Bind(configuration.GetSection(GreetingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FeatureOptions>()
            .Bind(configuration.GetSection(FeatureOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<FeatureOptions>, FeatureOptionsValidator>();

        return services;
    }
}
