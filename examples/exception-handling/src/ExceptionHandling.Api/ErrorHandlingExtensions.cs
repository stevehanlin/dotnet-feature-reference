using System.Diagnostics;

namespace ExceptionHandling.Api;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddAppErrorHandling(this IServiceCollection services)
    {
        // Applies to every problem details response: exceptions, status code pages, Results.Problem(...).
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["traceId"] = traceId;
            });

        // Order matters: specific handlers first, the catch-all last.
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static WebApplication UseAppErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();   // uses the registered IExceptionHandler chain
        app.UseStatusCodePages();    // empty 404/405/... responses become problem details too
        return app;
    }
}
