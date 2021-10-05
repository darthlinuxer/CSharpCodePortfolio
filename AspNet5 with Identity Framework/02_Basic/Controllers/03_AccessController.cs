using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using System.Threading.Channels;
using System.Threading.Tasks;
using App.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Configuration;

namespace App.Controllers
{

    public record LoginData
    {
        public string login;
        public string password;
    }

    public class AccessControl: ControllerBase
    {       
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccessControl(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult NotLoggedMessage() => BadRequest(new {msg = "You must login First!"});

        public async Task<IActionResult> Login([FromBody] LoginData body)
        {
            var _user = await _userManager.FindByNameAsync(body.login);
            if (_user is null) return NotFound(new {msg="User does not exist! Please Register first!"});
            var result = await _signInManager.PasswordSignInAsync(_user, body.password, false, true);
            if (!result.Succeeded) return BadRequest(new {msg = "Wrong Password!"});            
            return Ok(new {user = _user});
        }

        public async Task<IActionResult> Register([FromBody] LoginData body)
        {
            var _user = new IdentityUser
            {
                Email = body.login,
                LockoutEnabled = true,
                PasswordHash = "MySecretPasswordHash",
                UserName = body.login,
                AccessFailedCount = 3
            };

            var result = await _userManager.CreateAsync(_user, body.password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new {msg = "User Created!", user = _user});
        }

    }
}