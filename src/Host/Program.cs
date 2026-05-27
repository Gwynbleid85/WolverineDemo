using CleanResult;
using JasperFx;
using Lamar.Microsoft.DependencyInjection;
using ManualWolverineHandlerRegistration;
using ManualWolverineHandlerRegistration.Application.Commands;
using SharedKernel;
using Todos;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();
builder.Services.AddLogging();
string[] assemblies = ["Todos", "SharedKernel", "SwaggerExamples", "ApiKafka1", "ApiKafka2"];

builder.Host.AddProjects(assemblies, builder.Configuration);
builder.Services.AddTestHandlers();
builder.Services.AddSwaggerGen();
builder.Services.AddSwagger("WolverineDemo", assemblies);
builder.Services.AddMarten(builder.Configuration);
builder.Services.AddTodos(builder.Configuration);

var app = builder.Build();
app.UseCors();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseSharedKernel();
app.MapGet("/test/custom", (IMessageBus bus) => bus.InvokeAsync<Result>(new TestCommand()));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

return await app.RunJasperFxCommands(args);
