using System;
using App.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class StartupDbContext
    {
       public static void Init(IServiceCollection services, IConfiguration config)
       {

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("MemoryDb");
            });    

            //Identity is going to manage authentication
            services.AddIdentity<IdentityUser,IdentityRole>(options =>{
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 2;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric=false;
                options.Password.RequireUppercase=false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail=true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();      
            
            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.ExpireTimeSpan = TimeSpan.FromHours(24);
                config.Cookie.HttpOnly = true;
                config.LoginPath = "/AccessControl/NotLoggedMessage";
            });
                    

       }
    }
}