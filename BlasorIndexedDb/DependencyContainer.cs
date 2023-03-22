using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorIndexedDb;
public static class DependencyContainer
{
    public static WebAssemblyHostBuilder AddBlazorIndexedDbContext<TContext>(this WebAssemblyHostBuilder builder) where TContext : StoreContext<TContext>
    {                          
        builder.Services.AddScoped<TContext>();
        return builder;
    }

    public static async Task<WebAssemblyHost> UseBlazorIndexedDbContext<TContext>(this WebAssemblyHost app) where TContext : StoreContext<TContext>
    {
        StoreContext<TContext> context = app.Services.GetRequiredService<TContext>();
        await context.Init();
        return app;
    }
}
