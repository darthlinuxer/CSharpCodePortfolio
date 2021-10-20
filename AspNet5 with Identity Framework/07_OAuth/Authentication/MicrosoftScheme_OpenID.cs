using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddMicrosoftOpenID(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
             authBuilder.AddOpenIdConnect("Microsoft-OpenID", config =>
                {
                     config.ClientId = configuration.GetValue<string>("Microsoft:ClientId");
                     config.ClientSecret = configuration.GetValue<string>("Microsoft:ClientSecret");
                     config.Authority = configuration.GetValue<string>("Microsoft:authority");
                     config.CallbackPath = configuration.GetValue<string>("Microsoft:OpenIDCallbackPath");
                     config.AccessDeniedPath = "/AccessControl/AccessDenied";   
                     config.ResponseType= OpenIdConnectResponseType.CodeIdToken;     
                     config.SaveTokens = true;    
                     config.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents()
                     {
                         OnTokenResponseReceived = context =>
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