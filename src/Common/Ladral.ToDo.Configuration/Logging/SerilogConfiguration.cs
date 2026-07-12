using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Ladral.ToDo.Configuration.Logging;

public static class SerilogConfiguration
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        
        return builder;
    }
}
