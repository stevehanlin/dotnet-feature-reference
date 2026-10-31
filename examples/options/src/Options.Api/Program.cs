using Options.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppOptions(builder.Configuration);

var app = builder.Build();

app.MapOptionsEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
