using System;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using OpenIDAppRazor.Models;
using OpenIDAppRazor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;

namespace OpenIDAppRazor.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly EmailService _email;
        private readonly IConfiguration _configuration;

        public UserController(
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

        [HttpPost]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> Register([FromBody] UserRegisterData body)
        {
            var _user = new UserModel
            {
                Email = body.Login,
                LockoutEnabled = true,
                PasswordHash = _configuration.GetSection("PasswordHash").Get<string>(),
                AccessFailedCount = 3,
                UserName = body.Login
            };

            var result = await _userManager.CreateAsync(_user, body.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(_user);
            var link = Url.Action(nameof(VerifyEmail), "OpenId", new { userId = _user.Id, emailToken }, Request.Scheme, Request.Host.ToString());
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">Click to Verify Account</a>", true);
            return Ok(new { msg = $"Email confirmation sent to {_user.Email}" });
        }

        [HttpPost]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ReSendConfirmationAccountEmail([FromBody] UserRegisterData body)
        {
            var _user = await _userManager.FindByEmailAsync(body.Login);
            if (_user is null) return BadRequest(new { msg = "User is not registered!" });
            if (_user.EmailConfirmed) return BadRequest(new { msg = "User account was already confirmed! Just login!" });
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(_user);
            var link = Url.Action(nameof(VerifyEmail), "OpenId", new { userId = _user.Id, emailToken }, Request.Scheme, Request.Host.ToString());
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">Click to Verify Account</a>", true);
            return Ok(new { msg = $"Email confirmation sent to {_user.Email}" });
        }

        [HttpPost]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> LoginAndReturnToken(
       [FromBody] UserRegisterData body,
       [FromServices] TokenTools handler)
        {
            var _user = await _userManager.FindByEmailAsync(body.Login);
            if (_user is null) return NotFound(new { msg = "User does not exist! Please Register first!" });
            //if (_user.EmailConfirmed == false) return Ok(new {error = "Please verify your account!"});
            PasswordVerificationResult passResult = _userManager.PasswordHasher.VerifyHashedPassword(_user, _user.PasswordHash, body.Password);
            if (passResult == 0) return BadRequest(new { msg = "Wrong Password!" });
            //Generate JWT Token 
            var claims = await _userManager.GetClaimsAsync(_user);
            var genericIdentity = new GenericIdentity(_user.Email);
            genericIdentity.AddClaims(claims);
            var token = handler.CreateIdToken(genericIdentity, audience: _user.Email);
            return Ok(new { id_token = token });
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

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass)
        {
            var _user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (_user is null) return BadRequest(new { error = "User not found!" });
            var result = await _userManager.ChangePasswordAsync(_user, oldPass, newPass);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { msg = $"Password for user {_user.Email} changed!" });
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var _user = await _userManager.FindByEmailAsync(email);
            if (_user is null) return BadRequest(new { msg = "email not found!" });
            var token = await _userManager.GeneratePasswordResetTokenAsync(_user);
            var link = Url.Action(nameof(ResetPassword), "OpenId", new { userId = _user.Id, token }, Request.Scheme, Request.Host.ToString());
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">{link}</a>", true);
            return Ok(new { msg = $"Password reset token sent to {_user.Email}" });
        }

        [Authorize]
        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ResetPassword(string token, string userId)
        {
            var _user = await _userManager.FindByIdAsync(userId);
            if (_user is null) return BadRequest(new { msg = "user not found!" });
            var newPassword = RandomPassword.Generate(6);
            var resetPassResult = await _userManager.ResetPasswordAsync(_user, token, newPassword);
            if (!resetPassResult.Succeeded) return BadRequest(new { msg = "Invalid Token!" });
            await _email.SendAsync(_user.Email, "New Password", $"new password = {newPassword}", true);
            return Ok(new { newPassword });
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var _user = await _userManager.FindByIdAsync(userId);
            if (_user is null) return BadRequest(new { msg = "User does not exist!" });
            var result = await _userManager.ConfirmEmailAsync(_user, code);
            if (!result.Succeeded) return BadRequest(new { msg = "Invalid code!" });
            return Ok(new { msg = "Account Confirmed! ", _user.Email });
        }

    }
}