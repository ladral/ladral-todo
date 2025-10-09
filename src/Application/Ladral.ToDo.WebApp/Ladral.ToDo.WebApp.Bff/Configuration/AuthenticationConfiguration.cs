using System.Security.Claims;
using Ladral.ToDo.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Ladral.ToDo.WebApp.Bff.Configuration;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = ".Ladral.ToDo.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, oidcOptions =>
            {
                ConfigureOpenIdConnect(oidcOptions, configuration);
            });

        // ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
        // a new access token when the current one expires, and reissue a cookie with the
        // new access token saved inside. If the refresh fails, the user will be signed
        // out. OIDC connect options are set for saving tokens and the offline access
        // scope.
        services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        services.AddAuthorization();

        return services;
    }

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions oidcOptions, IConfiguration configuration)
    {
        oidcOptions.Authority = configuration["Authentication:Authority"];
        oidcOptions.ClientId = configuration["Authentication:ClientId"];
        oidcOptions.ClientSecret = configuration["Authentication:ClientSecret"];
        oidcOptions.Resource = configuration["Authentication:Audience"];
        oidcOptions.Prompt = "consent";
        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        oidcOptions.SaveTokens = true;
        oidcOptions.GetClaimsFromUserInfoEndpoint = false;
        
        ConfigureScopes(oidcOptions);
        ConfigureClaimMappings(oidcOptions);
        ConfigureEvents(oidcOptions);
    }

    private static void ConfigureScopes(OpenIdConnectOptions oidcOptions)
    {
        oidcOptions.Scope.Clear();
        oidcOptions.Scope.Add("openid");
        oidcOptions.Scope.Add("profile");
        oidcOptions.Scope.Add("read:todo");
        oidcOptions.Scope.Add("write:todo");
    }

    private static void ConfigureClaimMappings(OpenIdConnectOptions oidcOptions)
    {
        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = "name";
        oidcOptions.TokenValidationParameters.RoleClaimType = "role";
    }

    private static void ConfigureEvents(OpenIdConnectOptions oidcOptions)
    {
        oidcOptions.Events = new OpenIdConnectEvents
        {
            OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
            OnTokenValidated = OnTokenValidated
        };
    }

    private static Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        if (!string.IsNullOrEmpty(context.Options.Resource))
        {
            context.TokenEndpointRequest.Resource = context.Options.Resource;
        }
        return Task.CompletedTask;
    }

    private static Task OnTokenValidated(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity) 
            return Task.CompletedTask;

        var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
        
        EnsureSubjectClaim(identity, logger);
        EnsureNameClaim(identity, logger);

        return Task.CompletedTask;
    }

    private static void EnsureSubjectClaim(ClaimsIdentity identity, ILogger? logger)
    {
        var subClaim = identity.FindFirst("sub");
        if (subClaim == null)
        {
            logger?.LogWarning("No 'sub' claim found in token");
        }
    }

    private static void EnsureNameClaim(ClaimsIdentity identity, ILogger? logger)
    {
        var nameClaim = identity.FindFirst("name");
        if (nameClaim == null)
        {
            var preferredUsername = identity.FindFirst("preferred_username")?.Value;
            var email = identity.FindFirst("email")?.Value;
            var nameValue = preferredUsername ?? email ?? "Unknown User";
            identity.AddClaim(new Claim("name", nameValue));
            logger?.LogDebug("Added name claim: {NameValue}", nameValue);
        }
    }
}