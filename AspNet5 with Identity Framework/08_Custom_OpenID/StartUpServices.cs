using App.Middlewares;
using App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartUpServices
    {
        public static void Init(IServiceCollection services)
        {
            services.AddScoped<SecretService>();
            services.AddScoped<TokenTools>();
            services.AddScoped<RequestLocalizationCookiesMiddleware>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
        }
    }
}