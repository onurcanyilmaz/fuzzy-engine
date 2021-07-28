using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace ElasticLoggingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateLogging();
            CreateWebHost(args);
        }

        private static void CreateWebHost(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal("Failed to start", ex);
            }
        }

        private static void CreateLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                                                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                          .AddJsonFile($"appsettings.{environment}.json", optional: true)
                                                          .Build();

            Log.Logger = new LoggerConfiguration()
                                                  .Enrich.FromLogContext()
                                                  .Enrich.WithMachineName()
                                                  .WriteTo.Debug()
                                                  .WriteTo.Console()
                                                  .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                                                  .Enrich.WithProperty("Environment", environment)
                                                  .ReadFrom.Configuration(configuration)
                                                  .CreateLogger();
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}--{environment?.ToLower().Replace(".", "-")}--{DateTime.UtcNow:yyyy-MM}"
            };
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
                })
                .UseSerilog();
    }
}
