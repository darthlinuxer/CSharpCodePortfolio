using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
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
                //     options.DefaultScheme = "Google-Cookie";
                // }
                )
                .AddCookie("Google-Cookie", config =>
                {
                    config.LoginPath = "/Google/GoogleLogin";
                    config.Cookie.Name = "Google.Cookie";
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme, config =>
                {
                    config.ClientId = Configuration.GetValue<string>("Google:client_id");
                    config.ClientSecret = Configuration.GetValue<string>("Google:client_secret");
                    config.AuthorizationEndpoint = Configuration.GetValue<string>("Google:auth_uri");
                    config.TokenEndpoint = Configuration.GetValue<string>("Google:token_uri");
                    //config.CallbackPath = "/signin-google";
                    config.Events = new OAuthEvents()
                    {
                        OnTicketReceived = context =>
                        {
                            foreach (var header in context.HttpContext.Request.Headers)
                            {
                                Console.WriteLine(header);
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie("OAuth-Cookie", config =>
                {
                    config.LoginPath = "/OAuth/Login";
                    config.Cookie.Name = "OAuth.Cookie";
                })
                .AddOAuth("OAuthScheme", config =>
                {
                    string server = "https://localhost:5001";
                    config.SaveTokens = true;
                    config.AuthorizationEndpoint = $"{server}/OAuth/Authorize";
                    config.CallbackPath = "/OAuth/CallBack";
                    config.TokenEndpoint = $"{server}/OAuth/Token";
                    config.ClientId = "client_id";
                    config.ClientSecret = "client_secret";
                    config.Events = new OAuthEvents()
                    {
                        OnTicketReceived = context =>
                        {
                            foreach (var header in context.HttpContext.Request.Headers)
                            {
                                Console.WriteLine(header);
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie(IdentityConstants.ApplicationScheme, config =>
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
                            Console.WriteLine("Token:" + token.RawData);
                            ClaimsPrincipal principalInToken = tokenTools.ExtractPrincipal(token.RawData);

                            var _userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();

                            Console.WriteLine("User Id:" + context.Principal.Identity.Name);
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

            services.AddAuthorization(config =>
            {
                config.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("Google-Cookie")
                .AddAuthenticationSchemes("OAuth-Cookie")
                .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

                config.AddPolicy("Bearer", policyBuilder =>
                    policyBuilder
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build()
                     );

                config.AddPolicy("IdentityCookie", policyBuilder =>
                    policyBuilder
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build()
                     );

            });
        }
    }
}