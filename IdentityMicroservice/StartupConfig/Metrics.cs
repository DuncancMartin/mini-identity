using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Timer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IdentityApi.StartupConfig
{
    public static class Metrics
    {
        public static TimerOptions TokenEndpoint => new TimerOptions
        {
            Name = "Token Endpoint",
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds
        };

        //Extension method for IWebHostBuilder. 
        public static IHostBuilder ImagineMetrics(this IHostBuilder host)
        {
            host.ConfigureMetricsWithDefaults(builder =>
                {
                    builder.OutputMetrics.AsPrometheusPlainText();
                    builder.OutputMetrics.AsPrometheusProtobuf();
                })
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointsOptions =>
                    {
                        endpointsOptions.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                        endpointsOptions.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
                    };
                });
            return host;
        }
    }
}
