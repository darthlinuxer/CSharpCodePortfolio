using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

namespace App.JWTAuthentication
{
    public static class StartUpAuthentication
    {
        public static void Init(
            IServiceCollection services,
            IConfiguration Configuration
            )
        {
            var tokenTools = new TokenTools(Configuration);
            services.AddAuthentication(
                // options =>
                // {
                //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                // }
                )
                .AddCookie(IdentityConstants.ApplicationScheme,config =>
                {
                    config.Cookie.Name = "Identity.Cookie";
                    config.ExpireTimeSpan = TimeSpan.FromHours(24);
                    config.Cookie.HttpOnly = true;
                    config.LoginPath = "/AccessControl/NotLoggedMessage";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.SlidingExpiration = true;
                    config.Events = new CookieAuthenticationEvents
                    {
                        OnSignedIn = context =>
                        {
                            Console.WriteLine("OnCookieSignedInEvent Called! ");
                            return Task.CompletedTask;
                        },
                        OnValidatePrincipal = context =>
                        {
                            Console.WriteLine("OnCookieValidatePrincipal Called!");
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
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

                        OnTokenValidated = async context =>
                        {
                            Console.WriteLine("OnTokenValidated Called! ");

                            JwtSecurityToken token = context.SecurityToken as JwtSecurityToken;
                            var tokenTools = context.HttpContext.RequestServices.GetRequiredService<TokenTools>();
                            Console.WriteLine("Token:"+token.RawData);
                            ClaimsPrincipal principalInToken = tokenTools.ExtractPrincipal(token.RawData);

                            var _userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();

                            Console.WriteLine("User Id:"+context.Principal.Identity.Name);
                            var _user = await _userManager.FindByIdAsync(principalInToken.Identity.Name);
                            
                            var _claims = await _userManager.GetClaimsAsync(_user);
                            var genericIdentity = new GenericIdentity(_user.Id);
                            var userIdentity = new ClaimsIdentity(genericIdentity, _claims);
                            var identity = new ClaimsPrincipal(userIdentity);
                            context.HttpContext.User.AddIdentity(userIdentity);
                            context.Principal.AddIdentity(userIdentity);
                            Thread.CurrentPrincipal = identity;
                        },
                        OnAuthenticationFailed = options =>
                        {
                            options.Response.WriteAsJsonAsync(new { error = " Authentication Failed!" });
                            return Task.CompletedTask;
                        },
                        OnForbidden = options =>
                        {
                            options.Response.WriteAsJsonAsync(new { error = "Forbidden!" });
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = options =>
                        {
                            Console.WriteLine("OnMessageReceived Called! ");
                            return Task.CompletedTask;
                        }

                    };
                });
        }
    }
}