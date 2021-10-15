using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class AuthRoutesController: ControllerBase
    {
        public class Msg {public string msg;}
        
        [Authorize]
        public IActionResult Query(string msg) => Ok(new {msg});

        [Authorize("IdentityCookie")]
        public IActionResult QueryWithCookie(string msg) => Ok(new {msg});

        [Authorize("Bearer")]
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


        [Authorize("Security.5")]
        public IActionResult QueryWithSecureLevel(string msg) => Ok(new {msg});

        [Authorize("Security.10")]
        public IActionResult QueryWithHigherSecureLevel(string msg) => Ok(new {msg});

    }
}
