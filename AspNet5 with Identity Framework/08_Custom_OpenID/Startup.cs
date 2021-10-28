using System;
using System.Reflection;
using App.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;

[assembly: ResourceLocation("Resources")]
[assembly: RootNamespace("App")]

namespace App
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
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenID App", Version = "v1" });
            });

            services.AddLocalization(config => config.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SetDefaultCulture("pt-BR");
                options.AddSupportedUICultures("pt-BR","en-US", "de-DE", "ja-JP");
                
                options.FallBackToParentUICultures = true;
                options.RequestCultureProviders
                        .Remove(typeof(AcceptLanguageHeaderRequestCultureProvider));

            });

            StartupDbContext.Init(services);
            StartupEmail.Init(services, Configuration);
            StartUpServices.Init(services);
            StartUpIdentity.Init(services);
            StartUpAuthentication.Init(services, Secret);
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenID App v1"));
            }

            string[] supportedCultures = { "pt-br", "en-us", "de", "es", "it" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(config => config.ApplyCurrentCultureToResponseHeaders = true);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                Console.WriteLine("Logged User Id:" + context.User?.Identity?.Name);
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
