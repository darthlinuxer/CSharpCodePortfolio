using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    //Only Authorized Users can access these routes
    [Authorize]
    public class AuthRoutesController: ControllerBase
    {
        public class Msg {public string msg;}
        public IActionResult Query(string msg) => Ok(new {msg});
        public IActionResult Model([FromBody] Msg body) => Ok(new {body.msg});
        public IActionResult ModelWithoutAttributes(Msg body) => Ok(new {body.msg});
    }
}
