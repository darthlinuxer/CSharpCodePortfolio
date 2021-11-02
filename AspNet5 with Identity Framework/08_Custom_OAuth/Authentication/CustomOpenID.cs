using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OAuthApp
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddCustomOpenID(this AuthenticationBuilder authBuilder)
        {
            authBuilder.AddOpenIdConnect("OpenID", config =>
                {
                    string server = "https://localhost:5001";
                    config.ClientId = "client_id";
                    config.ClientSecret = "client_secret";
                    config.Authority = $"{server}/OpenIdExternal/Authority";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.CallbackPath = "/OpenID/CallBack";
                    config.SaveTokens = true;
                    config.MetadataAddress = $"{server}/.well-known/openid-configuration";
                    config.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidIssuer = "Reason Systems",
                        ValidateIssuer = true                        
                    };
                    config.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnAuthorizationCodeReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnTicketReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnTokenResponseReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnUserInformationReceived = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnAccessDenied = context =>
                        {
                            context.HandleResponse();
                            context.Response.WriteAsJsonAsync(new { msg = "Access Denied!", context.Result.Failure });
                            return Task.CompletedTask;
                        }

                    };
                });
            return authBuilder;
        }
    }
}