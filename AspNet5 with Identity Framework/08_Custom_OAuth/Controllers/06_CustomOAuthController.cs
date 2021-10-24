using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class OAuth : ControllerBase
    {
        public readonly UserManager<OAuthUser> _userManager;
        private string Code { get; set; }

        public OAuth(
            UserManager<OAuthUser> userManager
        )
        {
            this._userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        public IActionResult Test() => Ok(new { msg = "You are authorized to read this!" });
        public async Task<IActionResult> Authorize
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
            var code = (scope!="")? scope.EncodeTo64(): "Read";            
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

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
            var token = tokenTool.CreateToken(genericIdentity, audience: user.Client_ID);
            return Ok(new{access_token=token});
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
            var genericIdentity = new GenericIdentity(_user.Client_ID);
            var token = handler.CreateToken(genericIdentity, audience: _user.Client_ID);
            return Ok(new { id_token = token });
        }

        public async Task<IActionResult> Register([FromBody] UserRegisterData body)
        {
            var _user = new OAuthUser
            {
                Email = body.login,
                LockoutEnabled = true,
                PasswordHash = "MySecretPasswordHash",
                AccessFailedCount = 3,
                Client_ID = Guid.NewGuid().ToString(),
                Client_Secret = RandomPassword.Generate(10)
            };

            _user.UserName = _user.Client_ID;

            var result = await _userManager.CreateAsync(_user, body.password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddClaimsAsync(_user, new List<Claim>
            {
                new Claim("Client_ID", _user.Client_ID)
            });

            var secret = new Models.OAuthRecord()
            {
                Oauth_client_id = _user.Client_ID,
                Oauth_client_secret = _user.Client_Secret
            };
            //optional
            FileTools.WriteJsonToFile<OAuthRecord>(
                secret,
                @"c:\users\chave\dev\oauth_secrets.json"
            );

            return Ok(new { msg = "User Created!", user = _user });
        }

        [Authorize(AuthenticationSchemes = "JWT-Token")]
        public async Task<IActionResult> GetSecret([FromServices] TokenTools tokenTool)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null) return BadRequest(new { msg = "User does not exist!" });
            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(claims);
        }

    }

}