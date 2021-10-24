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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Org.BouncyCastle.Tsp;

namespace App
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
                    config.Authority = $"{server}/OpenID/Authority";
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