using AutoMapper;
using IdentityApi.Middleware;
using IdentityCore.Models.Settings;
using IdentityInfrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Linq;
using IdentityApi.StartupConfig;
using Imagine.Queue.Core.Interfaces;
using Imagine.Queue.RabbitMQ.Senders;
using k3imagine.StartupConfig.Swagger;
using System.Threading.Tasks;
using System.Threading;
using System;
using OpenIddict.Abstractions;
using IdentityCore.Models;

namespace IdentityApi
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private Settings _settings;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetValue<string>("IdentityDbConnectionString"));

                options.UseOpenIddict<CustomApplication, CustomAuthorization, CustomScope, ExtendedOpenIddictEntityFrameworkCoreToken, long>();
            });

            var redisHost = Configuration.GetValue<string>("RedisHost");
            var redisPort = Configuration.GetValue<int>("RedisPort");
            var redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");

            services.AddDataProtection()
                .SetApplicationName("Imagine")
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

            services.AddImagineIdentityConfig();
            services.AddAuthentication();

            services.AddControllers()
                .AddMetrics();

            services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VV");
            services.AddApiVersioning(o => o.ReportApiVersions = true);

            services.AddCors();
            services.AddAutoMapper(typeof(Startup));
            // Inject
            services.AddInjection();


            services.Configure<Settings>(Configuration);
            var container = services.BuildServiceProvider();
            _settings = container.GetService<IOptions<Settings>>().Value;

            ////Common sending service as all events put into this microservice's exchange
            //services.AddScoped<IQueueSender, RabbitSender>(provider =>
            //        new RabbitSender(_settings.RabbitSettings.Hostname,
            //                    _settings.RabbitSettings.Username,
            //                    _settings.RabbitSettings.Password));

            //ConfigureEventListeners(services);
            
            services.ConfigureSwagger(options =>
            {
                options.Title = "Identity Microservice";
                options.RemoveVersionParameters = true;
                options.SetVersionInPaths = true;
                options.UseFullTypeNameInSchemaIds = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            //This will create the DB if it does not exist.
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                context.Database.Migrate();
                context.EnsureSeedData(Configuration.GetValue<string>("UrlPostfix"));
            }

            app.ConfigureSwaggerAndBasePath(provider, Configuration.GetValue<string>("BasePath"));

            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

            app.UseAuthentication();
            app.UseAuthorization();

            //  Eventually only allow from k3 approved sources
            //  IHostingEnvironment can be used to further limit access
            app.UseCors(o => o.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseEndpoints(i => i.MapControllers());
            AddPublicapiToApplicationDatabaseAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task AddPublicapiToApplicationDatabaseAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("publicApi", cancellationToken) == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "publicApi",
                        DisplayName = "Public API",
                        PostLogoutRedirectUris = { new Uri("https://oidcdebugger.com/debug") },
                        RedirectUris = { new Uri("https://oidcdebugger.com/debug") },
                        ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342"
                    };

                    await manager.CreateAsync(descriptor, cancellationToken);
                }
            }
        }

        //private void ConfigureEventListeners(IServiceCollection services)
        //{
        //    // Create intance of item message handler
        //    var container = services.BuildServiceProvider();
        //    var commandParser = container.GetService<ICommandParser>();
        //    var logService = container.GetService<ILogService>();
        //    var rboMessageHandler = new RboMessageHandler(commandParser, container, logService);

        //    var itemListenerSettings = _settings.RabbitSettings?.ListenerSettings?.FirstOrDefault(s => s.Key == "ShopListener");
        //    if (itemListenerSettings.HasValue)
        //    {
        //        // Register item listener
        //        services.AddRabbitMqListener<RboMessageHandler, RabbitMessageModel>(settings =>
        //        {
        //            settings.ConnectionRetries = _settings.RabbitSettings.ReconnectionRetries;
        //            settings.ConnectionRetryWait = _settings.RabbitSettings.ReconnectionTime;
        //            settings.HostName = _settings.RabbitSettings.Hostname;
        //            settings.Password = _settings.RabbitSettings.Password;
        //            settings.Username = _settings.RabbitSettings.Username;
        //            settings.QueueName = itemListenerSettings.Value.Value.QueueName;
        //            settings.RoutingKey = itemListenerSettings.Value.Value.RoutingKey;
        //            settings.DurableQueue = itemListenerSettings.Value.Value.DurableQueue;
        //            settings.ExchangeName = itemListenerSettings.Value.Value.ExchangeName;
        //            settings.ExchangeType = itemListenerSettings.Value.Value.ExchangeType;
        //        }, rboMessageHandler);
        //    }
        //}
    }
}