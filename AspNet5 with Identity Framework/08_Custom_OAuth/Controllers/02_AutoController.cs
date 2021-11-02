using Microsoft.AspNetCore.Mvc;

namespace OAuthApp.Controllers
{
    //This Controller does not follow the Naming Convention 
    //nor does it have [ApiController] attribute
    public class Auto: ControllerBase
    {
        public class Msg {public string msg;}
        public IActionResult Query(string msg) => Ok(new {msg});
        public IActionResult Model([FromBody] Msg body) => Ok(new {body.msg});
        public IActionResult ModelWithoutAttributes(Msg body) => Ok(new {body.msg});

    }
}