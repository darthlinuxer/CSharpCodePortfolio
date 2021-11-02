using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace OAuthApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasicController : ControllerBase
    {
        public record Message {public string msg;}

        [HttpGet]
        [Route("[action]")]
        public IActionResult Index() => Ok(new {msg = "Hello World!"});

        [HttpGet]
        [Route("[action]/{msg}")]
        public IActionResult Route([FromRoute] string msg) => Ok(new {msg});
        
        [HttpGet]
        [Route("[action]/{msg}")]
        public IActionResult RouteWithouAttributes(string msg) => Ok(new {msg});

        [HttpGet]
        [Route("[action]")]
        public IActionResult Query([FromQuery] string msg) => Ok(new {msg});

        [HttpGet]
        [Route("[action]")]
        public IActionResult QueryWithoutAttributes(string msg) => Ok(new {msg});

        [HttpPost]
        [Route("[action]")]
        public IActionResult Body([FromBody] string msg) => Ok(msg);

        [HttpPost]
        [Route("[action]")]
        public IActionResult BodyDeSerialize([FromBody] string model) 
        {
            dynamic JsonObject = JsonConvert.DeserializeObject(model);
            return Ok(JsonObject?.msg??"There is no field msg");
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult BodyDeSerializeWithModel([FromBody] string model) 
        {
            Message msg = JsonConvert.DeserializeObject<Message>(model);
            return Ok(msg);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Model([FromBody] Message body) => Ok(new {body.msg});

         [HttpPost]
        [Route("[action]")]
        public IActionResult ModelWithoutAttributes(Message rcrd) => Ok(new {rcrd.msg});

        [HttpPost]
        [Route("[action]")]
        public IActionResult Header(
            [FromHeader] string msg1, 
            [FromHeader] string msg2) => Ok(new {msg1,msg2});
     
    }
}
