using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Compact;

namespace IdentityApi.StartupConfig
{
    public static class Logging
    {
        //Extension method for IWebHostBuilder. 
        public static IWebHostBuilder ImagineLogging(this IWebHostBuilder host)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(new ConfigurationBuilder().AddEnvironmentVariables().Build())
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();

            host.UseSerilog(logger);

            return host;
        }
    }
}
