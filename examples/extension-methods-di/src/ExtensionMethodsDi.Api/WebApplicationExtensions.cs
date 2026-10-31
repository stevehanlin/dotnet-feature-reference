namespace ExtensionMethodsDi.Api;

public static class WebApplicationExtensions
{
    // Pipeline setup grouped behind one call, same idea as AddXxx for services.
    public static IApplicationBuilder UseRequestStamp(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Stamped"] = "true";
            await next();
        });

    // Endpoint mapping grouped the same way.
    public static IEndpointRouteBuilder MapAppEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/greet/{name}", (string name, IGreeter greeter) => greeter.Greet(name));
        app.MapGet("/audit/{message}", (string message, IAuditLog log) => log.Format(message));
        return app;
    }
}

// 4. Static extension member (C# 14): adds a "static" method to a type you don't own.
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder)
    {
        public static WebApplicationBuilder CreateWithDefaults(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddClock();
            return builder;
        }
    }
}
