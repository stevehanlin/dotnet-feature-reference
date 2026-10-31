using ResultProblemDetails.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ProductService>();

var app = builder.Build();

app.UseStatusCodePages();
app.MapProductEndpoints();

app.Run();

// Exposes Program to WebApplicationFactory<T> in the test project.
public partial class Program;
