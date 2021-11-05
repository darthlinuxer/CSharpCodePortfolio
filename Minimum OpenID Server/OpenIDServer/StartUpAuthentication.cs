using OpenIDApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIDApp
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
                .AddAccessTokenScheme()                          
            ;

            services.AddAuthorization(config =>
            {
                config.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("JWT-Token")     
                .AddAuthenticationSchemes("Access-Token-Scheme")           
                .RequireAuthenticatedUser()
                .Build();             
            });
        }
    }
}