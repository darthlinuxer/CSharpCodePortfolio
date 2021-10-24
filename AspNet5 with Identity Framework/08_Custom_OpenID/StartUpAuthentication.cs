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
            SecretService secret
            )
        {
            TokenTools tokenTools = new(secret);
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "JWT-Token";
                options.DefaultChallengeScheme = "JWT-Token";
            })
                .AddJwtScheme(secret)                                   
            ;

            services.AddAuthorization(config =>
            {
                config.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("JWT-Token")                
                .RequireAuthenticatedUser()
                .Build();             
            });
        }
    }
}