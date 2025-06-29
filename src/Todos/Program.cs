using Lamar.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLamar();

var app = builder.Build();

app.Run();
