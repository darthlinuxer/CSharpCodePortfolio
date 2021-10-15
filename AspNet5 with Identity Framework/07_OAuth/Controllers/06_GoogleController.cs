using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class GoogleController: ControllerBase
    {
        [Authorize(AuthenticationSchemes = "Google-Cookie")]
        public IActionResult Test()
        {
            return Ok(new {msg = "Google Authorized this Endpoint!"});
        }
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync("Google-Cookie");
            var claims = result.Principal.Identities.FirstOrDefault()
                .Claims.Select( claim => new 
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });
            return Ok(claims);
        }
    }
}