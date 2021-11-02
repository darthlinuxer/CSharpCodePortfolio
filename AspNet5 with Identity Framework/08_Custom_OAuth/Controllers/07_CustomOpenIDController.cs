using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using OAuthApp.Models;
using OAuthApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace OAuthApp.Controllers
{
    public class CustomOpenIDController : ControllerBase
    {
        public readonly UserManager<OAuthUser> _userManager;

        public CustomOpenIDController(
            UserManager<OAuthUser> userManager
        )
        {
            this._userManager = userManager;
        }

        [Route(".well-known/openid-configuration")]
        public IActionResult GetOpenIDFile()
        {
            var dir = Directory.GetCurrentDirectory()+@"\Controllers\";
            var openId = FileTools.ReadJsonFromFile<OpenIDModel>(dir+"openid-configuration.json");
            return Ok(openId);
        }

        [Route("OpenId/[action]")]
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
            var code = (scope!="")? scope.EncodeTo64(): "Read";            
            return Redirect($"{redirect_uri}?state={state}&code={code}");
        }

        [Route("OpenId/[action]")]
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
    }
}