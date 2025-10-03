using System.Net.Http.Json;
using BlazorWebAppOidc.Client.Weather;

namespace Ladral.ToDo.WebApp.Client.Weather;

internal sealed class ClientWeatherForecaster(HttpClient httpClient) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync() =>
        await httpClient.GetFromJsonAsync<WeatherForecast[]>("api/weatherforecast") ??
        throw new IOException("No weather forecast!");
}