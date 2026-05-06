using System.Globalization;
using System.Reflection;
using CleanResult.Swashbuckle;
using CleanResult.WolverineFx;
using CommunityToolkit.Diagnostics;
using JasperFx.CodeGeneration;
using Marten;
using Microsoft.OpenApi;
using Serilog;
using SharedKernel.Infrastructure.Utils;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Kafka;
using Wolverine.Marten;
using Wolverine.Middleware;

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
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IHostBuilder AddProjects(
        this IHostBuilder host,
        string[] assemblies,
        IConfiguration configuration
    )
    {
        var kafkaUrl = configuration.GetValue<string>("Wolverine:Kafka:Url");
        var consumerGroupId = configuration.GetValue<string>("Wolverine:Kafka:ConsumerGroupId");
        Guard.IsNotNullOrEmpty(kafkaUrl, "Kafka Url not set.");
        Guard.IsNotNullOrEmpty(kafkaUrl, "Kafka Consumer group id not set.");

        host.UseWolverine(opts =>
        {
            foreach (var assembly in assemblies)
                opts.Discovery.IncludeAssembly(Assembly.Load(assembly));

            opts.Policies.AutoApplyTransactions();
            opts.Policies.UseDurableLocalQueues();
            opts.UseFluentValidation();
            opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
            opts.CodeGeneration.AddContinuationStrategy<CleanResultContinuationStrategy>();

            opts.UseKafka(kafkaUrl);

            opts.PublishToKafkaAutomatic(assemblies);

            opts.ListenToKafkaTopic("demo-test-topic")
                .ConfigureConsumer(config =>
                {
                    config.GroupId = consumerGroupId;
                });
        });

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} {CallerFilePath}|{CallerMemberName}:{CallerLineNumber}] {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture
            );
        Log.Logger = loggerConfiguration.CreateLogger();

        return host;
    }

    /// <summary>
    /// Add shared configuration to the project
    /// </summary>
    public static WebApplication UseSharedKernel(this WebApplication app)
    {
        app.MapWolverineEndpoints(opts =>
        {
            opts.UseFluentValidationProblemDetailMiddleware();
        });

        return app;
    }

    /// <summary>
    /// Configure Marten database
    /// </summary>
    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dbSchemeName = configuration.GetSection("DbSettings:MartenDb")["DatabaseNames"];
        var connectionString = configuration.GetSection("DbSettings:MartenDb")["ConnectionStrings"];
        Guard.IsNotNullOrEmpty(dbSchemeName, "Db scheme");
        Guard.IsNotNullOrEmpty(connectionString, "Connection string");

        services
            .AddMarten(opts =>
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
    public static IServiceCollection AddSwagger(
        this IServiceCollection services,
        string title,
        string[] assemblies
    )
    {
        services.AddSwaggerGen(options =>
        {
            // Configure basic swagger info
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = title,
                    Version = "v1",
                    Description = "CleanIAM API",
                }
            );

            // Add xml comments from all assemblies to swagger
            foreach (var assembly in assemblies)
            {
                var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly}.xml");
                options.IncludeXmlComments(assemblyXmlPath);
            }

            // Make all strings nullable by defaults
            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<FluentValidationSchemaFilter>();
            options.AddCleanResultFilters();
        });

        return services;
    }

    public static void PublishToKafkaAutomatic(this WolverineOptions opts, string[] assemblies)
    {
        var loadedAssemblies = assemblies.Select(Assembly.Load).ToArray();

        foreach (var assembly in loadedAssemblies)
        {
            opts.Discovery.IncludeAssembly(assembly);
        }

        foreach (
            var messageType in loadedAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    typeof(IKafkaMessage).IsAssignableFrom(t)
                    && t is { IsAbstract: false, IsInterface: false }
                )
        )
        {
            var topic = messageType.GetCustomAttribute<KafkaTopicAttribute>()?.TopicName;

            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new InvalidOperationException(
                    $"{messageType.FullName} implements IKafkaMessage but has no KafkaTopicAttribute."
                );
            }

            opts.PublishMessage(messageType).ToKafkaTopic(topic);
        }
    }
}
