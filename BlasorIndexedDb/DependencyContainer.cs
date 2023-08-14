namespace BlazorIndexedDb;

/// <summary>
/// Extension methods to add the dependencies
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Add context sing IServiceCollection
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="isSingleton">indicate teh server must be added like singleton. Default Scoped</param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorIndexedDbContext<TContext>(this IServiceCollection services, bool isSingleton = false) where TContext : StoreContext<TContext>
    {                
        if (isSingleton) services.AddSingleton<TContext>(); 
        else services.AddScoped<TContext>();
        return services;
    }

    /// <summary>
    /// Add context using WebAssemblyHostBuilder
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="builder"></param>    
    /// <param name="isSingleton">indicate teh server must be added like singleton. Default Scoped</param>
    /// <returns></returns>
    public static WebAssemblyHostBuilder AddBlazorIndexedDbContext<TContext>(this WebAssemblyHostBuilder builder, bool isSingleton = false) where TContext : StoreContext<TContext>
    {                          
        builder.Services.AddBlazorIndexedDbContext<TContext>(isSingleton);
        return builder;
    }

    /// <summary>
    /// Set ready to use the context in WebAssembly applications
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="app"></param>
    /// <returns></returns>
    public static async Task<WebAssemblyHost> UseBlazorIndexedDbContext<TContext>(this WebAssemblyHost app) where TContext : StoreContext<TContext>
    {
        StoreContext<TContext> context = app.Services.GetRequiredService<TContext>();
        await context.Init();
        return app;
    } 
}
