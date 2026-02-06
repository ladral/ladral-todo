using Ladral.ToDo.WebApp.Services;

namespace Ladral.ToDo.WebApp.Bff.Configuration;

public static class BffEndpointConfiguration
{
    public static IEndpointRouteBuilder MapBffEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapUserEndpoint(endpoints);
        MapAuthenticationEndpoints(endpoints);
        
        return endpoints;
    }
    private static void MapUserEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/user", (HttpContext context) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? false)
                return Results.Unauthorized();
            
            return Results.Ok(new
            {
                Name = context.User.Identity?.Name,
                IsAuthenticated = context.User.Identity?.IsAuthenticated,
                Claims = context.User.Claims.Select(c => new { c.Type, c.Value })
            });
        }).RequireAuthorization();
    }

    private static void MapAuthenticationEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/authentication").MapLoginAndLogout();
    }
}