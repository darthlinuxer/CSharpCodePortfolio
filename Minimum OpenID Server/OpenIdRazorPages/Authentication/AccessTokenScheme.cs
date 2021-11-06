using OpenIDAppRazor.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace OpenIDAppRazor
{
     public static partial class StartUpAuthentication
    {
        public static AuthenticationBuilder AddAccessTokenScheme(
            this AuthenticationBuilder authBuilder
            )
        {
            authBuilder.AddScheme<ValidateAccessTokenSchemeOptions, AccessTokenHandler>
            ("Access-Token-Scheme", op => 
            {
                op.SaveAccessToken = true;
            });
            
            return authBuilder;
        }
    }
}