using System;
using System.Text;
using System.Threading.Tasks;
using OAuthApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Tsp;

namespace OAuthApp
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddJwtScheme(
            this AuthenticationBuilder authBuilder,
            TokenTools tokenTools,
            IConfiguration configuration
            )
        {
            authBuilder.AddJwtBearer("JWT-Token", options =>
               {
                   options.RequireHttpsMetadata = false;
                   options.SaveToken = true;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidIssuer = "Reason Systems",
                       ValidateIssuer = true,
                       ValidateAudience = false,
                       IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("SecretKey"))),
                       ClockSkew = TimeSpan.Zero // remove delay of token when expire
                   };
                   options.Events = new JwtBearerEvents()
                   {
                       OnTokenValidated = context =>                       
                       {
                           return Task.CompletedTask;
                       },
                       OnMessageReceived = context =>
                       {
                           return Task.CompletedTask;
                       },
                       OnAuthenticationFailed = options =>
                       {
                           options.Response.WriteAsJsonAsync(new { error = "JWT Authentication Failed!" });
                           return Task.CompletedTask;
                       },
                       OnForbidden = options =>
                       {
                           options.Response.WriteAsJsonAsync(new { error = "JWT: Forbidden!" });
                           return Task.CompletedTask;
                       }
                   };
               });
            return authBuilder;
        }
    }
}