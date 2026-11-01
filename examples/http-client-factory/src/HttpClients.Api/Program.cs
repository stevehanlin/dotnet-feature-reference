using HttpClients.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuoteClients(builder.Configuration);

var app = builder.Build();

app.MapQuoteEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
