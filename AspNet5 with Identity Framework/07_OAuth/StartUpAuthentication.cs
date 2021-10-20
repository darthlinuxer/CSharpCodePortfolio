using System;
using App.TokenLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okta.AspNetCore;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static void Init(
            IServiceCollection services,
            IConfiguration configuration
            )
        {
            TokenTools tokenTools = new(configuration);
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
                .AddCookie(IdentityConstants.ApplicationScheme, config =>
                {
                    config.Cookie.Name = "Identity.Cookie";
                    config.LoginPath = "/AccessControl/NotLoggedMessage";
                    config.LogoutPath = "/AccessControl/Logout";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.ExpireTimeSpan = TimeSpan.FromHours(24);
                    config.Cookie.HttpOnly = true;
                    config.SlidingExpiration = true;
                })
                .AddJwtScheme(tokenTools)
                .AddCustomOAuthScheme()
                .AddGoogleScheme(configuration)                
                .AddOpenIdGoogleScheme(configuration)
                .AddMicrosoftScheme(configuration)   
                .AddMicrosoftOpenID(configuration)     
                .AddOktaOpenIDScheme()        
            ;

            services.AddAuthorization(config =>
            {
                config.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                .AddAuthenticationSchemes("JWT-Token")                
                .RequireAuthenticatedUser()
                .Build();             
            });
        }
    }
}