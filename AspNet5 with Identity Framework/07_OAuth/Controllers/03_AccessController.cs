using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using App.TokenLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace App.Controllers
{
    public class RandomPassword
    {
        public static string Generate(int length)
        {
            byte[] rgb = new byte[length];
            RNGCryptoServiceProvider rngCrypt = new();
            rngCrypt.GetBytes(rgb);
            return Convert.ToBase64String(rgb);
        }
    }
    public record UserRegisterData
    {
        public string login;
        public string password;
    }

    public class AccessControl : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly EmailService _email;

        public AccessControl(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            EmailService email)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _email = email;
        }

        public IActionResult ReadIdentityConstant() => Ok(new { IdentityConstants.ApplicationScheme });
        public IActionResult AlreadyLogged() => Ok(new { msg = "You are already Logged!" });
        public IActionResult AccessDenied() => BadRequest(new { msg = "Access Denied!" });
        public IActionResult NotLoggedMessage() => BadRequest(new { msg = "You must login first!" });

        public async Task<IActionResult> LoginAndReturnCookie(
            [FromBody] UserRegisterData body
            )
        {
            var _user = await _userManager.FindByEmailAsync(body.login);
            if (_user is null) return NotFound(new { msg = "User does not exist! Please Register first!" });
            var result = await _signInManager.PasswordSignInAsync(_user, body.password, false, true);
            if (!result.Succeeded) return BadRequest(new { msg = "Wrong Password!" });
            return Ok(_user);
        }

        [Route("[controller]/[action]/{provider}")]
        public IActionResult LoginWithProvider([FromRoute] string provider)
        {
            var server = HttpContext.Request.Host.Value;
            string http = HttpContext.Request.IsHttps == true ? "https" : "http";
            if (User != null && User.Identities.Any(identity => identity.IsAuthenticated))
            {
                return RedirectToAction("AlreadyLogged", "AccessControl");
            }
            var properties = new AuthenticationProperties { RedirectUri = $"{http}://{server}/AccessControl/ProviderResponse" };
            return Challenge(properties, provider);
            //return new ChallengeResult(provider, properties);
        }

        
        public async Task<IActionResult> ProviderResponse()
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (!result.Succeeded) return BadRequest(result.Failure);
            var claims = result?.Principal?.Identities?.FirstOrDefault()
                .Claims?.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value,
                });
            var id_token = result.Ticket.Properties.GetTokenValue("id_token");
            
            return Ok(new {id_token, claims});
        }

        public async Task<IActionResult> LoginAndReturnToken(
           [FromBody] UserRegisterData body,
           [FromServices] TokenTools handler)
        {
            var _user = await _userManager.FindByEmailAsync(body.login);
            if (_user is null) return NotFound(new { msg = "User does not exist! Please Register first!" });
            PasswordVerificationResult passResult = _userManager.PasswordHasher.VerifyHashedPassword(_user, _user.PasswordHash, body.password);
            if (passResult == 0) return BadRequest(new { msg = "Wrong Password!" });
            //Generate JWT Token 
            var _claims = await _userManager.GetClaimsAsync(_user);
            var genericIdentity = new GenericIdentity(_user.Id);
            genericIdentity.AddClaims(_claims);
            var token = handler.CreateToken(genericIdentity);
            return Ok(new { token });
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

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(_user);
            var link = Url.Action(nameof(VerifyEmail), "AccessControl", new { userId = _user.Id, code }, Request.Scheme, Request.Host.ToString());
            //await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">Click to Verify Account</a>" , true);             

            await _userManager.AddClaimsAsync(_user, new List<Claim>
            {
                new Claim("Role", "User"),
                new Claim("Email", _user.Email)
            });
            return Ok(new { msg = $"Email confirmation sent to {_user.Email}", user = _user });
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass)
        {
            var _user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(_user, oldPass, newPass);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { msg = $"Password for user {_user.Email} changed!" });
        }

        public async Task<IActionResult> ForgotPassword(string email)
        {
            var _user = await _userManager.FindByEmailAsync(email);
            if (_user is null) return BadRequest(new { msg = "email not found!" });
            var token = await _userManager.GeneratePasswordResetTokenAsync(_user);
            var link = Url.Action(nameof(ResetPassword), "AccessControl", new { userId = _user.Id, token }, Request.Scheme, Request.Host.ToString());
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">{link}</a>", true);
            return Ok(new { msg = $"Password reset token sent to {_user.Email}" });
        }

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

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var _user = await _userManager.FindByIdAsync(userId);
            if (_user is null) return BadRequest(new { msg = "User does not exist!" });
            var result = await _userManager.ConfirmEmailAsync(_user, code);
            if (!result.Succeeded) return BadRequest(new { msg = "Invalid code!" });
            return Ok(new { msg = "Account Confirmed! ", _user });
        }

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        public IActionResult WhoAmIFromToken()
        {
            return Ok(new { User.Identity });
        }

        [Authorize(AuthenticationSchemes = "Identity.Application")]
        public IActionResult WhoAmIFromCookie()
        {
            //string value = HttpContext.Request.Cookies["App.Cookie"];
            return Ok(User.Identity);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application")]
        public async Task<IActionResult> Logout()
        {
            var _userClaims = User.Claims;
            await HttpContext.SignOutAsync();
            return Ok(new {_userClaims , msg = "Logged out!" });
        }

    }
}