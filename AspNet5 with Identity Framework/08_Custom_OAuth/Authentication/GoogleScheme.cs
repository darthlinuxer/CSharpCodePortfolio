using System;
using System.Threading.Tasks;
using OAuthApp.Models;
using OAuthApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OAuthApp
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddGoogleScheme(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            authBuilder.AddGoogle("Google", config =>
                 {
                     config.ClientId = configuration.GetValue<string>("Google:client_id");
                     config.ClientSecret = FileTools.ReadJsonFromFile<ClientSecret>("c:\\users\\chave\\dev\\secrets.json")?.Google;
                     config.AuthorizationEndpoint = configuration.GetValue<string>("Google:auth_uri");
                     config.CallbackPath = "/Google/CallBack";
                     config.AccessDeniedPath = "/AccessControl/AccessDenied";
                     config.TokenEndpoint = configuration.GetValue<string>("Google:token_uri");
                     config.SaveTokens = true;
                     config.Events = new OAuthEvents
                     {
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