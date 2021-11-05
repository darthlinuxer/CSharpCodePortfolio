using System;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using OpenIDApp.Models;
using OpenIDApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;

namespace OpenIDApp.Controllers
{
    public class ValidationController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly EmailService _email;
        private readonly IConfiguration _configuration;

        public ValidationController(
            UserManager<UserModel> userManager,
            EmailService email,
            IConfiguration configuration
        )
        {
            this._userManager = userManager;
            this._email = email;
            this._configuration = configuration;
        }

        [HttpGet]
        public IActionResult AccessDenied() => BadRequest(new { msg = "Access Denied!" });
        [HttpGet]
        public IActionResult NotLoggedMessage() => BadRequest(new { msg = "You must login first!" });

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult TestWithIdToken() => Ok(new { msg = "You are authorized to read this endpoint with Id Token!" });

        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> TestWithAccessToken([FromServices] TokenTools tokenTools, string access_token)
        {
            var parts = TokenTools.DecodeTokenAndSeparateParts(access_token);
            var userName = parts[1];
            var _user = await _userManager.FindByEmailAsync(userName);
            if (_user is null) return BadRequest(new { error = "Token username does not exist!" });
            var isValid = tokenTools.IsTokenValid(access_token, Request.Host.Host, Request.Headers["User-Agent"]);
            if (!isValid) return BadRequest(new { error = "Token is not valid!" });
            return Ok(new { 
                msg = "You are authorized to read this endpoint with Access Token!",
                parts });
        }

        [Route(".well-known/openid-configuration")]
        [HttpGet]
        public IActionResult GetOpenIDFile()
        {
            var dir = Directory.GetCurrentDirectory() + @"\Controllers\";
            var openId = FileTools.ReadJsonFromFile<OpenIDModel>(dir + "openid-configuration.json");
            return Ok(openId);
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> RequestAccessToken(
            [FromServices] TokenTools tokenTools,
            [FromServices] SecretService secret)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null) return BadRequest(new { error = "Logged user not identified" });
            var CallerIP = HttpContext.Request.Host.Host;
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var access_token = tokenTools.CreateAccessToken(user.Email,
               secret.Get("PasswordHash"), CallerIP, userAgent, TimeSpan.TicksPerMinute * 10);
            return Ok(new { access_token });
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult ValidateAccessTokenFromQuery([FromServices] TokenTools tokenTools, string access_token)
        {            
            var isValid = tokenTools.IsTokenValid(
                access_token,
                HttpContext.Request.Host.Host,
                HttpContext.Request.Headers["User-Agent"].ToString()
                );
            return Ok(isValid);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access-Token-Scheme")]
        [Route("[controller]/[action]")]
        public IActionResult ValidateAccessTokenFromAttributes()
        {            
            return Ok(User.Identity);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> GetSecret()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (user is null) return BadRequest(new { msg = "User does not exist!" });
            return Ok(new { user.Email });
        }        

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> WhoAmIFromToken()
        {
            var _user = await _userManager.FindByEmailAsync(User.Identity.Name);
            return Ok(_user);
        }

    }
}