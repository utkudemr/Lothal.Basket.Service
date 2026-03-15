using Microsoft.Extensions.Hosting;
using Serilog;

namespace Lothal.BuildingBlocks.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddCustomLogging(this IHostApplicationBuilder builder, string applicationName)
    {
        builder.Services.AddSerilog((services, lc) =>
        {
            var victoriaLogsUrl = builder.Configuration["Serilog:WriteTo:1:Args:requestUri"]
                ?? "http://victorialogs:9428/insert/jsonline?_stream_fields=service,environment&_msg_field=@m&_time_field=@t";

            lc
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Yarp", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("service", applicationName)
                .Enrich.WithProperty("environment", builder.Environment.EnvironmentName)
                .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
                .WriteTo.Http(
                    requestUri: victoriaLogsUrl,
                    queueLimitBytes: null,
                    batchFormatter: new VictoriaLogsBatchFormatter(),
                    textFormatter: new Serilog.Formatting.Compact.CompactJsonFormatter());
        });

        return builder;
    }
}
