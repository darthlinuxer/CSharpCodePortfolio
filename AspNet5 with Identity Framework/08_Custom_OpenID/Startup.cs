using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpenIDApp.Middlewares;
using OpenIDApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;

[assembly: ResourceLocation("Resources")]
[assembly: RootNamespace("OpenIDApp")]

namespace OpenIDApp
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
            StartupDbCultures.Init(services);
            StartUpServices.Init(services);

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenID App", Version = "v1" });
            });

            services.AddLocalization();
           
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("pt-BR");

                var headerCultureProviderItem = options
                .RequestCultureProviders
                .Where(x => x is AcceptLanguageHeaderRequestCultureProvider)
                .FirstOrDefault();

                if (headerCultureProviderItem is not null) options.RequestCultureProviders.Remove(headerCultureProviderItem);

                options.AddInitialRequestCultureProvider(
                    new CustomRequestCultureProvider(async context =>
                {
                    if(options.SupportedCultures.Count > 1) return null;
                    var languageService = context.RequestServices.GetRequiredService<ILanguageService>();
                    var languages = await languageService.GetLanguages();
                    var cultures = languages.Select(x => new CultureInfo(x.Culture)).ToArray();
                    options.SupportedCultures = cultures;
                    options.SupportedUICultures = cultures;
                    return new ProviderCultureResult("pt-BR");
                })
                );
            });
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenID App v1"));
            }

            app.UseRequestLocalization();
            //app.UseRequestLocalizationCookies();

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

    public static class AppExtentionMethods
    {
        public static IApplicationBuilder UseRequestLocalizationCookies(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLocalizationCookiesMiddleware>();
        }
    }
}
