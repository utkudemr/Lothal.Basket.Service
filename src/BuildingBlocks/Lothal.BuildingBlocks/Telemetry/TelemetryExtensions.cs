using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace Lothal.BuildingBlocks.Telemetry;

public static class TelemetryExtensions
{
    public static IHostApplicationBuilder AddCustomTelemetry(this IHostApplicationBuilder builder, string applicationName)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://jaeger:4317";

                tracing
                    .AddSource(applicationName)
                    .AddSource("NATS.Net")
                    .AddSource("Couchbase.NetClient")
                    .AddSource("Yarp.ReverseProxy")
                    .AddSource("Npgsql")
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: applicationName, serviceVersion: "1.0.0"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            });

        return builder;
    }
}
