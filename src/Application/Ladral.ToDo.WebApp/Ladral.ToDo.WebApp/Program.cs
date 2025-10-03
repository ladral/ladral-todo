using Ladral.ToDo.WebApp.Client.Weather;
using Ladral.ToDo.WebApp.Components;
using Ladral.ToDo.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(options =>
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
        oidcOptions.Authority = builder.Configuration["Authentication:Authority"];
        oidcOptions.ClientId = builder.Configuration["Authentication:ClientId"];
        oidcOptions.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
        
        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        oidcOptions.SaveTokens = true;
        oidcOptions.GetClaimsFromUserInfoEndpoint = false;
        
        oidcOptions.Scope.Clear();
        oidcOptions.Scope.Add("openid");
        oidcOptions.Scope.Add("profile");
        oidcOptions.Scope.Add("api.todo");

        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = "name";
        oidcOptions.TokenValidationParameters.RoleClaimType = "role";
        
        oidcOptions.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                
                if (identity != null)
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                    
                    // Ensure we have a sub claim
                    var subClaim = identity.FindFirst("sub");
                    if (subClaim == null)
                    {
                        logger?.LogWarning("No 'sub' claim found in token");
                    }
                    
                    // Ensure we have a name claim
                    var nameClaim = identity.FindFirst("name");
                    if (nameClaim == null)
                    {
                        // Try to find alternative name claims
                        var preferredUsername = identity.FindFirst("preferred_username")?.Value;
                        var email = identity.FindFirst("email")?.Value;
                        var nameValue = preferredUsername ?? email ?? "Unknown User";
                        identity.AddClaim(new System.Security.Claims.Claim("name", nameValue));
                        logger?.LogInformation($"Added name claim: {nameValue}");
                    }
                }
                
                return Task.CompletedTask;
            }
        };
        
    });


// ConfigureCookieOidc attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. If the refresh fails, the user will be signed
// out. OIDC connect options are set for saving tokens and the offline access
// scope.
builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new("https://localhost:7173/api/weatherforecast");
});

// Add YARP reverse proxy for bff api
builder.Services.AddAuthenticatedReverseProxy(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Ladral.ToDo.WebApp.Client._Imports).Assembly);


// BFF API endpoints
app.MapGet("/user", (HttpContext context) =>
{
    if (!context.User.Identity?.IsAuthenticated ?? false)
        return Results.Unauthorized();
    
    return Results.Ok(new
    {
        Name = context.User.Identity.Name,
        IsAuthenticated = context.User.Identity.IsAuthenticated,
        Claims = context.User.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();

// Map reverse proxy for API calls
app.MapReverseProxy();

// BFF authentication endpoints
app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();