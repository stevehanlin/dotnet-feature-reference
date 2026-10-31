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
                context.ProblemDetails.Extensions["traceId"] =
                    ResolveTraceId(Activity.Current, context.HttpContext.TraceIdentifier);
            });

        // Order matters: specific handlers first, the catch-all last.
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    /// <summary>Prefers the W3C activity id (correlates with distributed traces); falls back to ASP.NET's per-request id.</summary>
    public static string ResolveTraceId(Activity? activity, string fallbackTraceIdentifier) =>
        activity?.Id ?? fallbackTraceIdentifier;

    public static WebApplication UseAppErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();   // uses the registered IExceptionHandler chain
        app.UseStatusCodePages();    // empty 404/405/... responses become problem details too
        return app;
    }
}
