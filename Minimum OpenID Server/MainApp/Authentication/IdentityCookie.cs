using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddIdentityScheme(this AuthenticationBuilder authBuilder, IConfiguration configuration)
        {
            authBuilder.AddCookie(IdentityConstants.ApplicationScheme, config =>
                 {
                    config.Cookie.Name = "Identity.Cookie";
                    config.LoginPath = "/AccessControl/NotLoggedMessage";
                    config.LogoutPath = "/AccessControl/Logout";
                    config.AccessDeniedPath = "/AccessControl/AccessDenied";
                    config.ExpireTimeSpan = TimeSpan.FromHours(24);
                    config.Cookie.HttpOnly = true;
                    config.SlidingExpiration = true;
                 });
            return authBuilder;
        }
    }
}