using BlazorWebAppOidc.Client.Weather;
using Ladral.ToDo.WebApp.Client.Weather;
using Microsoft.AspNetCore.Authentication;

namespace Ladral.ToDo.WebApp.Services;

internal sealed class ServerWeatherForecaster(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        var httpContext = httpContextAccessor.HttpContext ??
                          throw new InvalidOperationException("No HttpContext available from the IHttpContextAccessor!");
        var accessToken = await httpContext.GetTokenAsync("access_token") ??
                          throw new InvalidOperationException("No access_token was saved");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        request.Headers.Authorization = new("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
               throw new IOException("No weather forecast!");
    }
}