using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.Security.Cryptography;
using System;
using System.Security.Principal;
using App.TokenLib;

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

    public class AccessControl: ControllerBase
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

        public IActionResult AccessDenied() => BadRequest(new {msg = "Access Denied!"});
        public IActionResult NotLoggedMessage() => BadRequest(new {msg = "You must login First!"});

        public async Task<IActionResult> LoginAndReturnCookie(
            [FromBody] UserRegisterData body
            )
        {
            var _user = await _userManager.FindByEmailAsync(body.login);
            if (_user is null) return NotFound(new {msg="User does not exist! Please Register first!"});
            PasswordVerificationResult passResult = _userManager.PasswordHasher.VerifyHashedPassword(_user, _user.PasswordHash, body.password);
            if (passResult==0) return BadRequest(new {msg = "Wrong Password!"});
            
            //Identity will sign in and create a cookie
            await _signInManager.PasswordSignInAsync(_user, body.password, false, true);            
            return Ok(_user);
        }

         public async Task<IActionResult> LoginAndReturnToken(
            [FromBody] UserRegisterData body,
            [FromServices] TokenTools handler)
        {
            var _user = await _userManager.FindByEmailAsync(body.login);
            if (_user is null) return NotFound(new {msg="User does not exist! Please Register first!"});
            PasswordVerificationResult passResult = _userManager.PasswordHasher.VerifyHashedPassword(_user, _user.PasswordHash, body.password);
            if (passResult==0) return BadRequest(new {msg = "Wrong Password!"});            
            //Generate JWT Token without Claims
            var genericIdentity = new GenericIdentity(_user.Id);
            var userIdentity = new ClaimsIdentity(genericIdentity);
            var token = handler.CreateToken(userIdentity);
            
            return Ok(new {userIdentity, token });
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
            var link = Url.Action(nameof(VerifyEmail), "AccessControl", new {userId = _user.Id, code}, Request.Scheme, Request.Host.ToString());            
            //await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">Click to Verify Account</a>" , true);             

            await _userManager.AddClaimsAsync(_user, new List<Claim>
            {
                new Claim("Role", "User"),
                new Claim("Email", _user.Email)
            });
            return Ok(new {msg = $"Email confirmation sent to {_user.Email}", user = _user});
        }

        //Calls default authorization with is JWTBearer
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass)
        {
            var _user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(_user, oldPass, newPass);
            if(!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new {msg = $"Password for user {_user.Email} changed!"});
        }

        public async Task<IActionResult> ForgotPassword(string email)
        {
            var _user = await _userManager.FindByEmailAsync(email);
            if(_user is null) return BadRequest(new {msg = "email not found!"});
            var token = await _userManager.GeneratePasswordResetTokenAsync(_user);
            var link = Url.Action(nameof(ResetPassword), "AccessControl", new {userId = _user.Id, token}, Request.Scheme, Request.Host.ToString());            
            await _email.SendAsync(_user.Email, "Email Verification", $"<a href=\"{link}\">{link}</a>" , true); 
            return Ok(new {msg = $"Password reset token sent to {_user.Email}"});
        }

        public async Task<IActionResult> ResetPassword(string token, string userId)
        {
            var _user = await _userManager.FindByIdAsync(userId);
            if(_user is null) return BadRequest(new {msg = "user not found!"});
            var newPassword = RandomPassword.Generate(6);
            var resetPassResult = await _userManager.ResetPasswordAsync(_user, token, newPassword);
            if(!resetPassResult.Succeeded) return BadRequest(new {msg = "Invalid Token!"});
            await _email.SendAsync(_user.Email, "New Password", $"new password = {newPassword}" , true); 
            return Ok(new {newPassword});
        }

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var _user = await _userManager.FindByIdAsync(userId);
            if (_user is null) return BadRequest(new {msg = "User does not exist!"});
            var result = await _userManager.ConfirmEmailAsync(_user, code);
            if (!result.Succeeded) return BadRequest(new {msg = "Invalid code!"});
            return Ok(new {msg = "Account Confirmed! ", _user});            
        }

        [Authorize]
        public async Task<IActionResult> WhoAmIFromToken()
        {
            var user = await _userManager.FindByIdAsync(User.Identity.Name);
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(new {_user = user, _claims = claims, User.Identity});
        }

        [Authorize(Policy = "IdentityCookie")]
        public async Task<IActionResult> WhoAmIFromCookie()
        {
            var user = await _userManager.GetUserAsync(User);
            return Ok(new {id = User.Identity.Name, user});
        }

        [Authorize(Policy = "IdentityCookie")]
        public async Task<IActionResult> Logout()
        {
             var _user = await _userManager.GetUserAsync(User);
             await _signInManager.SignOutAsync();
             return Ok(new {user = _user?.Email, msg = "Logged out!"});
        }

    }
}