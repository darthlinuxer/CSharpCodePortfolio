using System;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddCustomOAuthScheme(this AuthenticationBuilder authBuilder)
        {
            authBuilder.AddOAuth("OAuth", config =>
                {
                    string server = "https://localhost:5001";
                    config.ClientId = "client_id";
                    config.ClientSecret = "client_secret";
                    config.AuthorizationEndpoint = $"{server}/OAuth/Authorize";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.CallbackPath = new PathString("/OAuth/CallBack");
                    config.TokenEndpoint = $"{server}/OAuth/Token";
                    config.UserInformationEndpoint = $"{server}/OAuth/UserInformation";
                    config.SaveTokens = true;
                    config.Events = new OAuthEvents()
                    {
                        OnCreatingTicket = context =>
                        {
                            Console.WriteLine("OnCreating Ticket!");
                            // TokenTools tokenTools = context.HttpContext.RequestServices.GetRequiredService<TokenTools>();
                            // var principal = tokenTools.ExtractPrincipal(context.AccessToken);
                            // context.Principal = principal;
                            return Task.CompletedTask;
                         },
                        OnTicketReceived = context =>
                        {
                            Console.WriteLine("OnTicket received!");
                            return Task.CompletedTask;
                        },
                        OnAccessDenied = context =>
                        {
                            context.HandleResponse();
                            context.Response.WriteAsJsonAsync(new {msg = "Access Denied!", context.Result.Failure});
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            context.HandleResponse();
                            context.Response.WriteAsJsonAsync(new {msg = "Remote Failed!", context.Result.Failure});
                            return Task.CompletedTask;
                        }

                    };
                });
            return authBuilder;
        }
    }
}