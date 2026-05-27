using System.Reflection;
using ManualWolverineHandlerRegistration.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace ManualWolverineHandlerRegistration;

public static class DependencyInjection
{
    public static IServiceCollection AddTestHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(Assembly.Load("ManualWolverineHandlerRegistration"));
            opts.Discovery.IncludeType<TestCommandHandler>();
        });
        return serviceCollection;
    }
}
