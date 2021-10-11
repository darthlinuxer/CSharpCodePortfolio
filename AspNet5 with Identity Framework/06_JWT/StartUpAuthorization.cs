using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace App.JWTAuthentication
{
    public static class StartUpAuthorization
    {
        public static void Init(
            IServiceCollection services, 
            IConfiguration Configuration
            )
        {
            var tokenTools = new App.TokenTools.TokenTools(Configuration);
            services.AddAuthentication(
                    options => {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                ).AddJwtBearer(x => {          
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenTools.tokenParameters;
                    }
                );

            services.AddScoped<App.TokenTools.TokenTools>();
        }
    }
}