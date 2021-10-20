using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;
using Okta.AspNetCore;

namespace App
{
    public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddOktaOpenIDScheme(this AuthenticationBuilder authBuilder)
        {
            authBuilder.AddOktaWebApi(new OktaWebApiOptions()
                {
                    OktaDomain = "https://dev-9366943.okta.com"                                  
                });
            return authBuilder;
        }
    }
}