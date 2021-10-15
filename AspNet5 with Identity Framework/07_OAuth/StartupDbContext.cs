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

        }
    }
}