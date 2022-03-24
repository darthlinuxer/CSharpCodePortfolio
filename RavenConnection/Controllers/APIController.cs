using Microsoft.AspNetCore.Mvc;

namespace RavenConnection.API
{
    [ApiController]
    [Route("[controller]")]
    public class APIController : ControllerBase
    {
        public APIController()
        {
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Get()
        {
            var message = "Server is Up!";
            return Ok(message);
        }

    }
}