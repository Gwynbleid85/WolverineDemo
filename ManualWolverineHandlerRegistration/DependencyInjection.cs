using System.Reflection;
using ManualWolverineHandlerRegistration.Application.Commands;
using ManualWolverineHandlerRegistration.Application.Interfaces;
using ManualWolverineHandlerRegistration.Core;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace ManualWolverineHandlerRegistration;

public static class DependencyInjection
{
    /// <summary>
    /// Register email template managemnt (endpoints, marten schemas, commands, ... )
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddEmailManagement(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureWolverine(opts =>
        {
            // Register Endpoints
            opts.Discovery.IncludeAssembly(Assembly.Load("ManualWolverineHandlerRegistration"));
        });

        // Register schema
        serviceCollection.ConfigureMarten(opts =>
        {
            opts.Schema.For<EmailTemplate>();
        });

        serviceCollection.AddScoped<IEmailManagementService>();
        return serviceCollection;
    }

    /// <summary>
    /// Register everything that the identity needs for email.
    /// Identity will directly use ony `IIdentityEmailService`
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddIdentityEmailService(
        this IServiceCollection serviceCollection
    )
    {
        // register only required event handler for template sync
        serviceCollection.ConfigureWolverine(opts =>
        {
            opts.Discovery.IncludeType<TestCommandHandler>();
        });

        // Register schema
        serviceCollection.ConfigureMarten(opts =>
        {
            opts.Schema.For<EmailTemplate>();
        });

        // Register email service per app
        serviceCollection.AddScoped<IIdentityEmailService>();

        return serviceCollection;
    }
}
