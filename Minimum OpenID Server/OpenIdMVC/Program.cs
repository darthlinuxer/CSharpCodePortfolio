using System;
using OpenIDAppMVC.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenIDAppMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // using var scope = host.Services.CreateScope();
            // var services = scope.ServiceProvider;
            // try
            // {
            //     var configuration = services.GetRequiredService<IConfiguration>();
            //     var secret = services.GetRequiredService<SecretService>();
            //     var tokenTools = services.GetRequiredService<TokenTools>();                
            // }
            // catch (Exception ex)
            // {
            //     var logger = services.GetRequiredService<ILogger<Program>>();
            //     logger.LogError(ex, "An error occurred");
            // }

            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
