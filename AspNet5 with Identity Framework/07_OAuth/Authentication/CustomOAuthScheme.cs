using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
                    config.CallbackPath = "/OAuth/CallBack";
                    config.TokenEndpoint = $"{server}/OAuth/Token";
                    config.SaveTokens = true;
                    config.Events = new OAuthEvents()
                    {         
                        OnRedirectToAuthorizationEndpoint = context =>
                        {

                            var secret = FileTools.ReadJsonFromFile<OAuthRecord>(@"c:\users\chave\dev\oauth_secrets.json");
                            context.Options.ClientId = secret.Oauth_client_id;
                            context.Options.ClientSecret = secret.Oauth_client_secret;

                            var query = new System.Uri(context.RedirectUri).Query;
                            var dict = System.Web.HttpUtility.ParseQueryString(query);
                            
                            var queryBuild = new QueryBuilder()
                            {
                                {"client_id",context.Options.ClientId},
                                {"scope","Read,Write"},
                                {"response_type","code"},
                                {"redirect_uri",context.Options.CallbackPath},
                                {"state",dict["state"]}
                            };

                            context.Response.Redirect($"{context.Options.AuthorizationEndpoint}{queryBuild.ToString()}");
                            return Task.CompletedTask;
                        },       
                        OnCreatingTicket = context =>
                        {
                            Console.WriteLine("OnCreating Ticket!");
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
                            context.Response.WriteAsJsonAsync(new { msg = "Access Denied!", context.Result.Failure });
                            return Task.CompletedTask;
                        }                      

                    };
                });
            return authBuilder;
        }
    }
}