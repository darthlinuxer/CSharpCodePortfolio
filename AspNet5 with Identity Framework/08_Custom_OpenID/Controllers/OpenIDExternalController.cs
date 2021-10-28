using System;
using System.Security.Principal;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class OpenIdExternalController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        public OpenIdExternalController(
            [FromServices] UserManager<UserModel> userManager
        )
        {
            this._userManager = userManager;
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
            var user = await _userManager.FindByIdAsync(client_id);
            if (user is null) return BadRequest(new { msg = "Client ID not registered! Please, register the App first to receive a client_id and client_secret!" });
            var code = (scope != "") ? scope.EncodeTo64() : "Read";
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

        [HttpGet]
        public async Task<IActionResult> Token(
                    string client_id,
                    string client_secret,
                    string code,
                    string state,
                    [FromServices] TokenTools tokenTool,
                    [FromServices] SecretService secret
                )
        {
            var user = await _userManager.FindByIdAsync(client_id);
            if (user is null) return BadRequest(new { msg = "User does not exist!" });

            var _claims = await _userManager.GetClaimsAsync(user);
            var genericIdentity = new GenericIdentity(user.Id);
            genericIdentity.AddClaims(_claims);
            //var id_token = tokenTool.CreateIdToken(genericIdentity, audience: user.Email);
            var access_token = tokenTool.CreateAccessToken(
                user.Email,
                secret.Get("PasswordHash"),                
                HttpContext.Request.Host.Host,
                HttpContext.Request.Headers["User-Agent"].ToString() ?? "User-Agent",
                TimeSpan.TicksPerMinute*10
                );
            return Ok(new { access_token });
        }

    }
}