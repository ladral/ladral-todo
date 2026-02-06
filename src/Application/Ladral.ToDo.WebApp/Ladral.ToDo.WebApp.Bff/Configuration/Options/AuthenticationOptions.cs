namespace Ladral.ToDo.WebApp.Bff.Configuration.Options;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Cookie configuration options
    /// </summary>
    public CookieOptions Cookie { get; set; } = new();
    
    /// <summary>
    /// OpenID Connect specific options
    /// </summary>
    public OpenIdConnectConfigurationOptions OpenIdConnectConfiguration { get; set; } = new();
}

public class CookieOptions
{
    public string Name { get; set; } = ".Ladral.ToDo.Auth";
    public string LoginPath { get; set; } = "/login";
    public string LogoutPath { get; set; } = "/logout";
}

public class OpenIdConnectConfigurationOptions
{
    public string Prompt { get; set; } = "consent";
    public string ResponseType { get; set; } = "code";
    
    public List<string> Scopes { get; set; } = new()
    {
        "openid",
        "profile",
        "read:todo",
        "write:todo"
    };
}
