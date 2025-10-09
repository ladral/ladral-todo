
using Ladral.ToDo.WebApp.Bff.Configuration.Options;
using Ladral.ToDo.WebApp.Client.Weather;
using Ladral.ToDo.WebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ladral.ToDo.WebApp.Bff.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure all strongly-typed options
        services.Configure<ApiEndpointsOptions>(configuration.GetSection(ApiEndpointsOptions.SectionName));
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        
        services.AddAuthenticationServices(configuration);
        services.AddBlazorServices();
        services.AddHttpClientServices(configuration);
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

    private static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiEndpoints = configuration.GetSection(ApiEndpointsOptions.SectionName).Get<ApiEndpointsOptions>();
        
        services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(apiEndpoints?.WeatherApi ?? throw new InvalidOperationException("WeatherApi endpoint is not configured"));
        });
        
        return services;
    }

    private static IServiceCollection AddReverseProxyServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add YARP reverse proxy for bff api
        services.AddAuthenticatedReverseProxy(configuration.GetSection(ReverseProxyOptions.SectionName));
        return services;
    }
}