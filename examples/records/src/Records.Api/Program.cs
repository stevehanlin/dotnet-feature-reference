using Records.Api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapOrderEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
