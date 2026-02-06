using BlazorWebAppOidc.Client.Weather;

namespace Ladral.ToDo.WebApp.Client.Weather;

public interface IWeatherForecaster
{
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}
