using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Ladral.ToDo.WebApp.Services;

public static class YarpExtensions
{
    public static IReverseProxyBuilder AddAuthenticatedReverseProxy(
        this IServiceCollection services, 
        IConfigurationSection configurationSection)
    {
        return services.AddReverseProxy()
            .LoadFromConfig(configurationSection)
            .AddTransforms(builderContext =>
            {
                builderContext.AddAuthenticationTransforms();
            });
    }
    
    public static void AddAuthenticationTransforms(this TransformBuilderContext builderContext)
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            var httpContext = transformContext.HttpContext;
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var accessToken = await httpContext.GetTokenAsync("access_token");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        // Add the Bearer token
                        transformContext.ProxyRequest.Headers.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                        
                        // Remove the Cookie header from the proxied request
                        transformContext.ProxyRequest.Headers.Remove("Cookie");
                    }
                }
                catch (Exception ex)
                {
                    var logger = httpContext.RequestServices.GetService<ILogger>();
                    logger?.LogError(ex, "Failed to retrieve access token for YARP transform");
                }
            }
        });
    }
}