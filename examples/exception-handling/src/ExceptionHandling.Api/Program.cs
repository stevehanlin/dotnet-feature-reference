using ExceptionHandling.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppErrorHandling();

var app = builder.Build();

app.UseAppErrorHandling();
app.MapDemoEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
