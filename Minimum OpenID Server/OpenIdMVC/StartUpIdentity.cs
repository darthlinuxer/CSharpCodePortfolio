using System;
using OpenIDAppMVC.Context;
using OpenIDAppMVC.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenIDAppMVC
{
    public class StartUpIdentity
    {
        public static void Init(IServiceCollection services)
        {
            //Identity is going to manage authentication
            services
            .AddIdentityCore<UserModel>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;

                    //Password Settings
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 2;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;

                    //Lockout Settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 3;

                    options.User.RequireUniqueEmail = true;
                })
            .AddSignInManager<SignInManager<UserModel>>()
            .AddUserManager<UserManager<UserModel>>()
            .AddEntityFrameworkStores<OpenIdDbContext>()
            .AddDefaultTokenProviders()
            ;
        }
    }
}