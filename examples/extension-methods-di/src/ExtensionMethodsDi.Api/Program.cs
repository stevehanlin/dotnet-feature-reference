using ExtensionMethodsDi.Api;

// Startup reads as a table of contents. Each line hides a feature's registration details.
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddClock()
    .AddGreeter(options => options.Greeting = "Hello")
    .AddAuditLog(builder.Configuration);

var app = builder.Build();

app.UseRequestStamp();
app.MapAppEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
