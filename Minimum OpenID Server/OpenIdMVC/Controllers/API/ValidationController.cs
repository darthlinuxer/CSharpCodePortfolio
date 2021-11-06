using System;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using OpenIDAppMVC.Models;
using OpenIDAppMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;
using System.Runtime.CompilerServices;

namespace OpenIDAppMVC.Controllers
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
        public async Task<IActionResult> ValidateAccessTokenFromAttributes()
        {   
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);         
            return Ok(user);
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