using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace App.Controllers
{
    public class OAuth : ControllerBase
    {
        public IActionResult Authorize
        (
            string client_id,
            string scope,
            string response_type,
            string redirect_uri,
            string state
        )
        {
            var code = "UserSecretCode";
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

        public IActionResult Token(
            string client_id,
            string client_secret,
            string redirect_uri,
            string code
        )
        {

            return Ok();
        }
    }
      
}