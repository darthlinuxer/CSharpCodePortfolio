using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddMicrosoftScheme(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
             authBuilder.AddMicrosoftAccount("Microsoft", config =>
                {
                     config.ClientId = configuration.GetValue<string>("Microsoft:ClientId");
                     config.ClientSecret = FileTools.ReadJsonFromFile<ClientSecret>("c:\\users\\chave\\dev\\secrets.json")?.Microsoft;
                     config.AuthorizationEndpoint = configuration.GetValue<string>("Microsoft:authorize");
                     config.TokenEndpoint = configuration.GetValue<string>("Microsoft:token");
                     config.CallbackPath = configuration.GetValue<string>("Microsoft:CallbackPath");
                     config.AccessDeniedPath = "/AccessControl/AccessDenied";         
                     config.SaveTokens = true;    
                     config.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
                     {
                         OnCreatingTicket = context =>
                         {
                             return Task.CompletedTask;
                         },
                         OnTicketReceived = context =>
                         {
                             return Task.CompletedTask;
                         }
                     };           
                });
            return authBuilder;
        }
    }
}