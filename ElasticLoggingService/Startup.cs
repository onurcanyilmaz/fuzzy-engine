using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ElasticLoggingService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ElasticLoggingService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElasticLoggingService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            app.Map("/map1", app =>
            {
                app.Run(async context =>
                {
                    var branchVer = context.Request.Query["branch"];

                    logger.LogInformation("map1 ~ branch info = " + branchVer);
                    await context.Response.WriteAsync($"Branch used = {branchVer}");
                });
            });

            app.Map("/map2", app =>
            {
                app.Run(async context =>
                {
                    string body = string.Empty;
                    var request = context.Request;
                    using (var reader
                                      = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                    {
                        body = await reader.ReadToEndAsync();
                    }
                    logger.LogInformation(body);
                    Console.WriteLine(body);
                });
            });

            app.Map("/map3", app =>
            {
                app.Run(async context =>
                {
                    try
                    {
                        throw new Exception("MyCustomError");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unknown error");
                    }
                    await context.Response.WriteAsync("Bir hata oluştu");
                });
            });
        }
    }
}
