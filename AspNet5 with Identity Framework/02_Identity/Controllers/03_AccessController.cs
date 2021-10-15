using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace App.Controllers
{

    public record UserRegisterData
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

        public async Task<IActionResult> Login([FromBody] UserRegisterData body)
        {
            var _user = await _userManager.FindByEmailAsync(body.login);
            if (_user is null) return NotFound(new {msg="User does not exist! Please Register first!"});
            var result = await _signInManager.PasswordSignInAsync(_user, body.password, false, true);
            if (!result.Succeeded) return BadRequest(new {msg = "Wrong Password!"});            
            return Ok(new {user = _user});
        }

        public async Task<IActionResult> Register([FromBody] UserRegisterData body)
        {
            var _user = new IdentityUser
            {
                Email = body.login,
                LockoutEnabled = true,
                PasswordHash = "MySecretPasswordHash",
                AccessFailedCount = 3,
                UserName = body.login
            };

            var result = await _userManager.CreateAsync(_user, body.password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddClaimsAsync(_user, new List<Claim>
            {
                new Claim(ClaimTypes.Role, "User")
            });
            return Ok(new {msg = "User Created!", user = _user});
        }

        [Authorize]
        public async Task<IActionResult> WhoAmI()
        {
            var _user = await _userManager.GetUserAsync(User);
            return Ok(_user);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var _user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            return Ok(new {user = _user?.Email, msg = "Logged out!"});
        }

    }
}