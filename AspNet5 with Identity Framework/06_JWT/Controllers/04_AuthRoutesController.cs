using System.Threading.Tasks;
using App.Attributes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class AuthRoutesController: ControllerBase
    {
        public class Msg {public string msg;}
        
        [Authorize("IdentityCookie")]
        [Authorize(Policy = "Role.User")]
        [Authorize(Policy = "Email")]
        public IActionResult QueryWithCookie(string msg) => Ok(new {msg});

        [Authorize("Bearer")]
        [Authorize(Policy = "Role.User")]
        [Authorize(Policy = "Email")]
        public IActionResult QueryWithBearer(string msg) => Ok(new {msg});

        public async Task<IActionResult> Model(
            [FromServices] IAuthorizationService authorizationService, 
            [FromBody] Msg body) 
            {   
                var builder = new AuthorizationPolicyBuilder("CustomPolicySchema");
                var customPolicy = builder.RequireClaim("Height").Build();
                var authResult = await authorizationService.AuthorizeAsync(User, customPolicy);

                if (!authResult.Succeeded) return BadRequest(
                    new { 
                        claimRequired = "A claim is missing!", 
                        claim = authResult.Failure
                        });

                return Ok(new {body.msg});
            } 
        public IActionResult ModelWithoutAttributes(Msg body) => Ok(new {body.msg});


        [Authorize]
        [Authorize(Policy = "Security.5")]
        public IActionResult QueryWithSecureLevel(string msg) => Ok(new {msg});

        [Authorize]
        [SecurityLevel(10)]
        public IActionResult QueryWithHigherSecureLevel(string msg) => Ok(new {msg});

    }
}
