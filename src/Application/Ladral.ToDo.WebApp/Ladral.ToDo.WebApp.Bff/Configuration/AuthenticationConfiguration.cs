using System.Security.Claims;
using Ladral.ToDo.WebApp.Bff.Configuration.Options;
using Ladral.ToDo.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using CookieOptions = Ladral.ToDo.WebApp.Bff.Configuration.Options.CookieOptions;

namespace Ladral.ToDo.WebApp.Bff.Configuration;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => { ConfigureCookieOptions(options, authenticationOptions.Cookie); })
            .AddOpenIdConnect(oidcOptions => { ConfigureOpenIdConnect(oidcOptions, authenticationOptions); });

        // ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
        // a new access token when the current one expires, and reissue a cookie with the
        // new access token saved inside. If the refresh fails, the user will be signed
        // out. OIDC connect options are set for saving tokens and the offline access
        // scope.
        services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        services.AddAuthorization();

        return services;
    }


    private static void ConfigureCookieOptions(CookieAuthenticationOptions cookieOptions, CookieOptions configOptions)
    {
        cookieOptions.Cookie.Name = configOptions.Name;
        cookieOptions.Cookie.HttpOnly = true;
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
        cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(1);
        cookieOptions.SlidingExpiration = true;
        cookieOptions.LoginPath = configOptions.LoginPath;
        cookieOptions.LogoutPath = configOptions.LogoutPath;
    }

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions oidcOptions, AuthenticationOptions authOptions)
    {
#if DEBUG
        oidcOptions.RequireHttpsMetadata = false;
#else
        oidcOptions.RequireHttpsMetadata = true;
#endif
        oidcOptions.Authority = authOptions.Authority;
        oidcOptions.ClientId = authOptions.ClientId;
        oidcOptions.ClientSecret = authOptions.ClientSecret;
        oidcOptions.Resource = authOptions.Audience;
        oidcOptions.Prompt = authOptions.OpenIdConnectConfiguration.Prompt;
        oidcOptions.ResponseType = authOptions.OpenIdConnectConfiguration.ResponseType;
        oidcOptions.SaveTokens = true;
        oidcOptions.GetClaimsFromUserInfoEndpoint = false;

        ConfigureScopes(oidcOptions, authOptions.OpenIdConnectConfiguration.Scopes);
        ConfigureClaimMappings(oidcOptions);
        ConfigureEvents(oidcOptions);
    }

    private static void ConfigureScopes(OpenIdConnectOptions oidcOptions, List<string> scopes)
    {
        oidcOptions.Scope.Clear();
        foreach (var scope in scopes)
        {
            oidcOptions.Scope.Add(scope);
        }
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