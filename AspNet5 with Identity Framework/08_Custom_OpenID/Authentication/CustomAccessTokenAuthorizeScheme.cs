using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Authentication
{
    public class ValidateAccessTokenSchemeOptions : AuthenticationSchemeOptions
    {
        public bool SaveAccessToken { get; set; }
        public ValidateAccessTokenSchemeOptions()
        {
        }
    }

    public class AccessTokenHandler
        : AuthenticationHandler<ValidateAccessTokenSchemeOptions>
    {
        private readonly TokenTools tokenTools;

        public AccessTokenHandler(
            IOptionsMonitor<ValidateAccessTokenSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            TokenTools tokenTools)
            : base(options, logger, encoder, clock)
        {
            this.tokenTools = tokenTools;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // validation comes in here
            if (!Request.Headers.ContainsKey("Access-Token"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Access-Token Not Found on Request Headers!"));
            }

            var access_token = Request.Headers["Access-Token"];
            var isValid = tokenTools.IsTokenValid(
                access_token,
                Request.Host.Host,
                Request.Headers["User-Agent"].ToString()
                );

            if (!isValid) Task.FromResult(AuthenticateResult.Fail("Access-Token Not Valid!"));

            var claimsIdentity = new GenericIdentity(TokenTools.DecodeTokenAndSeparateParts(access_token)[1]);

            // generate AuthenticationTicket from the Identity
            // and current authentication scheme
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}