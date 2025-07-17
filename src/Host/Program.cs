using Lamar.Microsoft.DependencyInjection;
using SharedKernel;
using Swashbuckle.FluentValidation.AspNetCore;
using Todos;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLamar();

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();
builder.Services.AddLogging();
string[] assemblies =
[
    "Todos",
    "SharedKernel",
    "SwaggerExamples"
];

builder.Host.AddProjects(assemblies);

builder.Services.AddSwaggerGen();
builder.Services.AddSwagger("WolverineDemo", assemblies);
builder.Services.AddMarten(builder.Configuration);
builder.Services.AddTodos(builder.Configuration);
builder.Services.AddFluentValidationRulesToSwagger();

var app = builder.Build();
app.UseCors();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseSharedKernel();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();