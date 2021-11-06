using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using OpenIDAppRazor.Services;

[assembly: ResourceLocation("Resources")]
[assembly: RootNamespace("OpenIDApp")]

namespace OpenIDAppRazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.Secret = new SecretService(configuration);
        }
        public IConfiguration Configuration { get; }
        public SecretService Secret { get; set; }
        public void ConfigureServices(IServiceCollection services)
        {
            StartupDbContext.Init(services);
            StartupEmail.Init(services, Configuration);
            StartUpIdentity.Init(services);
            StartUpAuthentication.Init(services, Secret);
            StartUpServices.Init(services);

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddRazorPages()
                .WithRazorPagesRoot("/Pages")
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });

        }
    }
}
