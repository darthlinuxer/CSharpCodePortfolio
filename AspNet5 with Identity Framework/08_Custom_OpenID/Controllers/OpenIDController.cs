using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;

namespace App.Controllers
{
    public class OpenIdController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly EmailService _email;
        private readonly IConfiguration _configuration;

        public OpenIdController(
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
        public IActionResult Test() => Ok(new { msg = "You are authorized to read this!" });

        [Route(".well-known/openid-configuration")]
        [HttpGet]
        public IActionResult GetOpenIDFile()
        {
            var dir = Directory.GetCurrentDirectory() + @"\Controllers\";
            var openId = FileTools.ReadJsonFromFile<OpenIDModel>(dir + "openid-configuration.json");
            return Ok(openId);
        }

        [HttpGet]
        public async Task<IActionResult> Authority
        (
            string client_id,
            string scope,
            string response_type,
            string redirect_uri,
            string state
        )
        {
            var user = await _userManager.FindByNameAsync(client_id);
            if (user is null) return BadRequest(new { msg = "App Client ID not registered! Please, register the App first to receive a client_id and client_secret!" });
            var code = (scope != "") ? scope.EncodeTo64() : "Read";
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

        [HttpGet]
        public async Task<IActionResult> Token(
                    string client_id,
                    string client_secret,
                    string code,
                    string state,
                    [FromServices] TokenTools tokenTool
                )
        {
            var user = await _userManager.FindByNameAsync(client_id);
            if (user is null) return BadRequest(new { msg = "User does not exist!" });
            if (client_secret != user.Client_Secret) return BadRequest(new { msg = "Invalid Secret!" });
            var _claims = await _userManager.GetClaimsAsync(user);
            var genericIdentity = new GenericIdentity(user.Id);
            genericIdentity.AddClaims(_claims);
            var id_token = tokenTool.CreateIdToken(genericIdentity, audience: user.Client_ID);
            var access_token = tokenTool.CreateAccessToken(
                user.Email,
                user.PasswordHash,
                HttpContext.Connection.LocalIpAddress.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString() ?? "User-Agent",
                10
                );
            return Ok(new { id_token, access_token });
        }

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
                Client_ID = Guid.NewGuid().ToString(),
                Client_Secret = RandomPassword.Generate(10)
            };

            _user.UserName = _user.Client_ID;

            var result = await _userManager.CreateAsync(_user, body.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddClaimsAsync(_user, new List<Claim>
            {
                new Claim("Client_ID", _user.Client_ID)
            });

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(_user);
            var link = Url.Action(nameof(VerifyEmail), "OpenId", new { userId = _user.Id, code }, Request.Scheme, Request.Host.ToString());
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">Click to Verify Account</a>", true);
            return Ok(new { msg = $"Email confirmation sent to {_user.Email}", user = _user });
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
            var genericIdentity = new GenericIdentity(_user.Client_ID);
            genericIdentity.AddClaims(claims);
            var token = handler.CreateIdToken(genericIdentity, audience: _user.Client_ID);
            return Ok(new { id_token = token });
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> RequestAccessToken([FromServices] TokenTools tokenTools)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null) return BadRequest(new { error = "Logged user not identified" });
            var localIP = HttpContext.Connection.LocalIpAddress.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString() ?? "User-Agent";
            var access_token = tokenTools.CreateAccessToken(user.Email,
               user.PasswordHash, localIP, userAgent, TimeSpan.TicksPerMinute*10 );
            return Ok(new {access_token});
        }

        [HttpGet]
        [Authorize]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ValidateAccessToken([FromServices] TokenTools tokenTools, string token)
        {
            var parts = TokenTools.DecodeTokenAndSeparateParts(token);
            if (parts.Length !=3) return BadRequest(new {error = "Token has wrong format"});
            var username = parts[1];
            var user = await _userManager.FindByEmailAsync(username);
            if (user is null) return BadRequest(new {error = "Access Token User is not registered!"});            
            var isValid = tokenTools.IsTokenValid(
                token,
                HttpContext.Connection.LocalIpAddress.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString() ?? "User-Agent", 
                user.PasswordHash
                );
            return Ok(isValid);
        }




        [HttpGet]
        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> GetSecret()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null) return BadRequest(new { msg = "User does not exist!" });
            return Ok(new { user.Client_Secret });
        }

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass)
        {
            var _user = await _userManager.FindByNameAsync(User.Identity.Name);
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
            return Ok(new { msg = "Account Confirmed! ", _user });
        }

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult WhoAmIFromToken()
        {
            return Ok(new { User.Identity });
        }

    }
}