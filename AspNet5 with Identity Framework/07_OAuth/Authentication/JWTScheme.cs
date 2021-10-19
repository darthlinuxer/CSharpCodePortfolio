using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddJwtScheme(
            this AuthenticationBuilder authBuilder,
            TokenTools tokenTools
            )
        {
            authBuilder.AddJwtBearer("JWT-Token", options =>
               {
                   options.RequireHttpsMetadata = true;
                   options.SaveToken = true;
                   options.TokenValidationParameters = tokenTools.tokenParameters;
                   options.Events = new JwtBearerEvents()
                   {
                       OnChallenge = context =>
                       {
                           context.HandleResponse();
                           context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                           context.Response.ContentType = "application/json";

                           // Ensure we always have an error and error description.
                           if (string.IsNullOrEmpty(context.Error))
                               context.Error = "invalid_token";
                           if (string.IsNullOrEmpty(context.ErrorDescription))
                               context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                           // Add some extra context for expired tokens.
                           if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                           {
                               var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                               context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                               context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
                           }

                           return context.Response.WriteAsync(JsonConvert.SerializeObject(new
                           {
                               error = context.Error,
                               error_description = context.ErrorDescription
                           }));
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