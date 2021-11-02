using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NETCore.MailKit.Core;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;

namespace OAuthApp
{
    public class StartupEmail
    {
       public static void Init(IServiceCollection services, IConfiguration _config)
       {
           services.AddScoped<EmailService>();
           services.AddMailKit(config =>
           {
               config.UseMailKit(_config.GetSection("Email").Get<MailKitOptions>());
           });          

       }
    }
}