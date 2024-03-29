using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;
using Org.BouncyCastle.Tsp;
using OAuthApp.Models;
using OAuthApp.Services;

namespace OAuthApp
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddOpenIdGoogleScheme(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            authBuilder.AddOpenIdConnect("Google-OpenID", config =>
                {
                     config.ClientId = configuration.GetValue<string>("Google:client_id");
                     config.ClientSecret = FileTools.ReadJsonFromFile<ClientSecret>("c:\\users\\chave\\dev\\secrets.json")?.Google;
                     config.Authority = "https://accounts.google.com";
                     config.CallbackPath = "/Google/CallBack-OpenID";
                     config.AccessDeniedPath = "/AccessControl/AccessDenied";                     
                     config.SaveTokens = true;
                     config.Events = new OpenIdConnectEvents()
                     {
                         OnTokenResponseReceived = context =>
                         {
                             return Task.CompletedTask;
                         },
                         OnTicketReceived = context =>
                         {
                             return Task.CompletedTask;
                         },
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