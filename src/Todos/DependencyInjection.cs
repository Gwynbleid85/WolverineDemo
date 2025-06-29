namespace Todos;

public static class DependencyInjection
{
    /// <summary>
    /// Configure Todos module
    /// </summary>
    public static IServiceCollection AddTodos(this IServiceCollection services, IConfiguration configuration)
    {

        return services;
    }

    /// <summary>
    /// Configure Todos module
    /// </summary>
    public static IApplicationBuilder UseTodos(this IApplicationBuilder app)
    {
        return app;
    }
}