using System;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
                    config.ClientId="later";
                    config.ClientSecret="later";
                    config.AuthorizationEndpoint = $"{server}/OAuth/Authorize";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.CallbackPath = new PathString("/OAuth/CallBack");
                    config.TokenEndpoint = $"{server}/OAuth/Token";
                    config.UserInformationEndpoint = $"{server}/OAuth/UserInformation";
                    config.SaveTokens = true;
                    config.Events = new OAuthEvents()
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                       {
                           var secrets = FileTools.ReadJsonFromFile<OAuthRecord>(
                               "c:\\users\\chave\\dev\\oauth_secrets.json"
                           );
                           context.Options.ClientId = secrets.Oauth_client_id;
                           context.Options.ClientSecret = secrets.Oauth_client_secret;
                        //    var authority = context.Options.AuthorizationEndpoint;
                        //    string client_id = secrets.Oauth_client_id;
                        //    string scope = "Read";
                        //    string response_type = "code";
                        //    string redirect_uri = context.Options.CallbackPath;

                        //    var query = new QueryBuilder
                        //    {
                        //        { "client_id", client_id },
                        //        { "scope", scope },
                        //        { "response_type", response_type },
                        //        { "redirect_uri", redirect_uri }
                        //    };

                           context.Response.Redirect(context.RedirectUri);
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
                        },
                        OnRemoteFailure = context =>
                        {
                            context.HandleResponse();
                            context.Response.WriteAsJsonAsync(new { msg = "Remote Failed!", context.Result.Failure });
                            return Task.CompletedTask;
                        }

                    };
                });
            return authBuilder;
        }
    }
}