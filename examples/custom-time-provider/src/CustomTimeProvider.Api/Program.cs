using CustomTimeProvider.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBusinessClock("America/New_York")
    .AddStoreServices();

var app = builder.Build();

app.MapStoreEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
