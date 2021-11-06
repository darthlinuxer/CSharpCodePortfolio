using System;
using App.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartUpIdentity
    {
        public static void Init(IServiceCollection services)
        {

            //Identity is going to manage authentication
            services
            .AddIdentityCore<IdentityUser>(options =>
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
            .AddSignInManager<SignInManager<IdentityUser>>()
            .AddUserManager<UserManager<IdentityUser>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }

    }
}