using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            app.UseHttpsRedirection();

            app.UseExceptionHandler(app => app.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";

                WriteExceptionRequestLog(new LogRequest
                {
                    Exception = exception,
                    LogLevel = LogLevel.Critical,
                    Message = exception.Message
                });

                await WriteAsync(context, result);
            }));


            app.Map("/write-log", app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Method != "POST")
                    {
                        await WriteAsync(context, "the method http verb must be a POST!");
                        return;
                    }

                    var body = context.Request.Body;
                    LogRequest logObj = await GetLogRequestObjectAsync(body);

                    if (logObj == null)
                    {
                        await WriteAsync(context, "the request body which is not valid");
                        return;
                    }

                    if (logObj.Secret != Configuration["Secret"])
                    {
                        await WriteAsync(context, "the secret key which is not valid");
                        return;
                    }

                    WriteRequestLog(logObj);
                    await WriteAsync(context, "succes!");
                });
            });

            async Task WriteAsync(HttpContext context, string message)
            {
                await context.Response.WriteAsync(message);
            }

            async Task<LogRequest> GetLogRequestObjectAsync(Stream body)
            {
                using (var reader = new StreamReader(body, Encoding.UTF8, true, 1024, true))
                {
                    return JsonConvert.DeserializeObject<LogRequest>(await reader.ReadToEndAsync());
                }
            }

            void WriteExceptionRequestLog(LogRequest request)
            {
                logger.Log(request.LogLevel, request.Exception, request.Message);
            }

            void WriteRequestLog(LogRequest request)
            {
                logger.Log(request.LogLevel, request.Message);
            }
        }
    }
}
