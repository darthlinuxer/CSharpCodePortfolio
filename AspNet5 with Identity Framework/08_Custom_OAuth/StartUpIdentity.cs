using System;
using OAuthApp.Context;
using OAuthApp.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OAuthApp
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

            //Identity is going to manage authentication
            services
            .AddIdentityCore<OAuthUser>(options =>
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
            .AddSignInManager<SignInManager<OAuthUser>>()
            .AddUserManager<UserManager<OAuthUser>>()
            .AddEntityFrameworkStores<OAuthDbContext>()
            .AddTokenProvider<PasswordResetTokenProvider<OAuthUser>>("passwordReset")
            ;
        }


        public class PasswordResetTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
        {
            public PasswordResetTokenProvider(IDataProtectionProvider dataProtectionProvider,
                IOptions<PasswordResetTokenProviderOptions> options,
                ILogger<DataProtectorTokenProvider<TUser>> logger)
                : base(dataProtectionProvider, options, logger)
            {

            }
        }

        public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
        {
            public PasswordResetTokenProviderOptions()
            {
                Name = "PasswordResetTokenProvider";
                TokenLifespan = TimeSpan.FromDays(3);
            }
        }
    }
}