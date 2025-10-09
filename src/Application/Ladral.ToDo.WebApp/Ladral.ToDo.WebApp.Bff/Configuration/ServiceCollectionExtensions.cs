using Ladral.ToDo.WebApp.Client.Weather;
using Ladral.ToDo.WebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ladral.ToDo.WebApp.Bff.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthenticationServices(configuration);
        services.AddBlazorServices();
        services.AddHttpClientServices();
        services.AddReverseProxyServices(configuration);
        
        return services;
    }

    private static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        
        services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();
        services.AddHttpContextAccessor();
        
        return services;
    }

    private static IServiceCollection AddHttpClientServices(this IServiceCollection services)
    {
        services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
        {
            httpClient.BaseAddress = new("https://localhost:7173/api/weatherforecast");
        });
        
        return services;
    }

    private static IServiceCollection AddReverseProxyServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add YARP reverse proxy for bff api
        services.AddAuthenticatedReverseProxy(configuration.GetSection("ReverseProxy"));
        return services;
    }
}