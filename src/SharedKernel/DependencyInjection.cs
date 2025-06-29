
using System.Reflection;
using CommunityToolkit.Diagnostics;
using JasperFx.CodeGeneration;
using Microsoft.OpenApi.Models;
using SharedKernel.Application.Swagger;
using Wolverine;
using Wolverine.FluentValidation;
using Marten;
using Wolverine.Marten;

namespace SharedKernel;

/// <summary>
/// Class containing methods for registering shared services or utils and configuration shared behaviour.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configure parts the project that need access to all assemblies (e.g. Wolverine)
    /// </summary>
    /// <param name="host"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IHostBuilder AddProjects(this IHostBuilder host, string[] assemblies)
    {
        host.UseWolverine(opts =>
        {
            foreach (var assembly in assemblies)
                opts.Discovery.IncludeAssembly(Assembly.Load(assembly));
            opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;

            opts.Policies.AutoApplyTransactions();
            opts.Policies.UseDurableLocalQueues();
            opts.UseFluentValidation();
            opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
        });

        return host;
    }

    /// <summary>
    /// Add shared configuration to the project
    /// </summary>
    public static WebApplication UseSharedKernel(this WebApplication app)
    {
        // Register middlewares

        return app;
    }
    

    /// <summary>
    /// Configure Marten database
    /// </summary>
    public static IServiceCollection AddMarten(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSchemeName = configuration.GetSection("DbSettings:MartenDb")["DatabaseNames"];
        var connectionString =
            configuration.GetSection("DbSettings:MartenDb")["ConnectionStrings"];
        Guard.IsNotNullOrEmpty(dbSchemeName, "Db scheme");
        Guard.IsNotNullOrEmpty(connectionString, "Connection string");

        services.AddMarten(opts =>
            {
                opts.Connection(connectionString);
                opts.DatabaseSchemaName = dbSchemeName;
                opts.Policies.AllDocumentsAreMultiTenanted();
            })
            .ApplyAllDatabaseChangesOnStartup()
            .UseLightweightSessions()
            .IntegrateWithWolverine();
        

        return services;
    }

    /// <summary>
    /// Configure swagger
    /// </summary>
    /// <param name="services"></param>
    /// <param name="title"> Title of the OpenApi schema</param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services, string title,
        string[] assemblies)
    {
        services.AddSwaggerGen(options =>
        {
            // Configure basic swagger info 
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = "CleanIAM API"
            });
            
            // Add xml comments from all assemblies to swagger
            foreach (var assembly in assemblies)
            {
                var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly}.xml");
                options.IncludeXmlComments(assemblyXmlPath);
            }

            // Make all strings nullable by defaults
            options.SupportNonNullableReferenceTypes();
            // Make all properties required by default (For nicer api generation on FE. See docs)
            options.SchemaFilter<MakeAllPropertiesRequiredFilter>();
        });

        return services;
    }
}