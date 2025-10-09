namespace Ladral.ToDo.WebApp.Bff.Configuration.Options;

public class ApiEndpointsOptions
{
    public const string SectionName = "ApiEndpoints";
    
    public string WeatherApi { get; set; } = string.Empty;
}