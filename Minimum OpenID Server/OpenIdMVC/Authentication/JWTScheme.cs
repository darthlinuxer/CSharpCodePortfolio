using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OpenIDAppMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace OpenIDAppMVC
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddJwtScheme(
            this AuthenticationBuilder authBuilder,
            SecretService secret
            )
        {
            authBuilder.AddJwtBearer("JWT-Token", options =>
               {
                   options.RequireHttpsMetadata = false;
                   options.SaveToken = true;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidIssuer = "OpenID_Server",
                       ValidateIssuer = true,
                       ValidateAudience = false,
                       IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret.Get("SecretKey"))),
                       ClockSkew = TimeSpan.Zero // remove delay of token when expire
                   };
                   options.Events = new JwtBearerEvents()
                   {
                       OnChallenge = context =>
                       {
                           context.HandleResponse();
                           context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                           context.Response.WriteAsJsonAsync(new {error = "Token Invalid or Unkown!"});
                           return Task.CompletedTask;
                       },
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