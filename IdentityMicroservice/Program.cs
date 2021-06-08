using IdentityApi.StartupConfig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IdentityApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //This UseKestrel & allowsynchronousIO should be removed once appmetrics solves it's async issues
                    //Maybe even look at removing AppMetrics
                    webBuilder.UseKestrel(o => o.AllowSynchronousIO = true);
                    webBuilder.SuppressStatusMessages(true);
                    webBuilder.ImagineLogging();
                    webBuilder.CaptureStartupErrors(false);
                    webBuilder.UseStartup<Startup>();
                })
                .ImagineMetrics();
    }
}
