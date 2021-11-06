using System;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                .AddJwtScheme(tokenTools, configuration)
                .AddCustomOpenID()
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