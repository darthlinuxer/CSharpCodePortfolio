using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class OAuthController:ControllerBase
    {
        [Authorize(AuthenticationSchemes = "OAuth-Cookie")]
        public IActionResult Test()
        {
            return Ok(new {msg = "OAuth Authorized Endpoint!"});
        }

         public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("OAuthResponse")
            };
            return Challenge(properties, "OAuthScheme");
        }

        public async Task<IActionResult> OAuthResponse()
        {
            var result = await HttpContext.AuthenticateAsync("OAuth-Cookie");
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

        public IActionResult Authorize(
            string client_id,
            string scope,
            string response_type,
            string redirect_uri,
            string state
        )
        {
            var code = "MyOwnSecretCode";
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

        public async Task<IActionResult> Token(
            string grant_type,
            string code,
            string redirect_uri,
            string client_id,
            string state,
            [FromServices] TokenTools tokenTool
        )
        {
            IList<Claim> _claims = new List<Claim>
            {
                new Claim("User","My User Name")
            };
            var genericIdentity = new GenericIdentity("My Generic Identity");
            var userIdentity = new ClaimsIdentity(genericIdentity, _claims);
            var access_token = tokenTool.CreateToken(userIdentity);

            // var responseObject = new {
            //     access_token,
            //     token_type="Bearer"
            // };
            // var responseJson = JsonConvert.SerializeObject(responseObject);
            // var responseBytes = Encoding.UTF8.GetBytes(responseJson);
            
            return Redirect($"{redirect_uri}");
        } 
    }
}