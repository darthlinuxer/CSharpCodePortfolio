using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddOktaOpenIDScheme(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            authBuilder.AddOpenIdConnect("Okta-OpenID", config =>
                {
                     config.ClientId = configuration.GetValue<string>("Google:client_id");
                     config.ClientSecret = configuration.GetValue<string>("Google:client_secret");
                     config.Authority = "https://accounts.google.com";
                     config.CallbackPath = "/Google/CallBack";
                     config.AccessDeniedPath = "/AccessControl/AccessDenied";                     
                     config.SaveTokens = true;
                     config.Events = new OpenIdConnectEvents()
                     {
                         OnTokenValidated = context =>
                         {
                             return Task.CompletedTask;
                         }
                     };
                });
            return authBuilder;
        }
    }
}