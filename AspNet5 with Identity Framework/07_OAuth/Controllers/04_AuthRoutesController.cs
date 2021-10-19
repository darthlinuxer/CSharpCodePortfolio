using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class AuthRoutesController: ControllerBase
    {
        public class Msg {public string msg;}
        
        public async Task<IActionResult> Query(
            [FromServices] IAuthorizationService authorizationService, 
            string msg) 
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync("Google-Cookie");
            if(!result.Succeeded) result = await HttpContext.AuthenticateAsync("OAuth-Cookie");
            if(!result.Succeeded) result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if(!result.Succeeded) result = await HttpContext.AuthenticateAsync("JWT-Token");
            if(!result.Succeeded) return BadRequest(new {msg = "Failed Authentication!"});

            var builder = new AuthorizationPolicyBuilder("CustomPolicySchema");
            var customPolicy = builder.RequireAuthenticatedUser().Build();
            var authResult = await authorizationService.AuthorizeAsync(User,customPolicy);
    

            if(!authResult.Succeeded) return BadRequest(new {msg = "User not authorized!", authResult.Failure, User.Claims});
            return Ok(new {
                successfulAuthorization = true,
                msg = "Query from manual Authentication and Authorization",
                principal = result.Principal.Identity, 
                userIdentity = User.Identity});
        }

        //Default Policy
        [Authorize]
        public IActionResult QueryDefault(string msg) => Ok(new {msg});

        [Authorize("IdentityPolicy")]
        public IActionResult QueryWithCookie(string msg) => Ok(new {msg});

        [Authorize("TokenPolicy")]
        public IActionResult QueryWithBearer(string msg) => Ok(new {msg});

        public async Task<IActionResult> Model(
            [FromServices] IAuthorizationService authorizationService, 
            [FromBody] Msg body) 
            {   
                var builder = new AuthorizationPolicyBuilder("CustomPolicy");
                var customPolicy = builder.RequireClaim("Height").Build();
                var authResult = await authorizationService.AuthorizeAsync(User, customPolicy);

                if (!authResult.Succeeded) return BadRequest(
                    new { 
                        claimRequired = "A claim is missing!", 
                        claim = authResult.Failure
                        });

                return Ok(new {body.msg});
            } 

        [Authorize]
        public IActionResult ModelWithoutAttributes(Msg body) => Ok(new {body.msg});


        [Authorize("Security(5)")]
        public IActionResult QueryWithSecureLevel(string msg) => Ok(new {msg});

        [Authorize("Security(10)")]
        public IActionResult QueryWithHigherSecureLevel(string msg) => Ok(new {msg});

    }
}
