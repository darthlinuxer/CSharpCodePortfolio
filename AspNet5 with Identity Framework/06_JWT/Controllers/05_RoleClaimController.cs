using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace App.Controllers
{
    [Authorize("Bearer")]
    public class RoleClaimController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public RoleClaimController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public record ClaimModel{public string claimType; public string value;}
        public async Task<IActionResult> AddClaim([FromBody] ClaimModel cModel)
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();

            var hasClaim = User.HasClaim(c=>c.Type == cModel.claimType);
            if(hasClaim) return BadRequest(new {error = $"Claim {cModel.claimType} already exist!"});
            
            var customClaim = new Claim(cModel.claimType, cModel.value);
            var result = await _userManager.AddClaimAsync(_user, customClaim);
            if (result.Succeeded) 
            {                
                await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Claim Added!", cModel});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> RemoveClaim([FromBody] ClaimModel cModel)
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();

            IList<Claim> userClaims = await _userManager.GetClaimsAsync(_user);
            var hasClaim = userClaims.FirstOrDefault(x=>x.Type == cModel.claimType);
            if(hasClaim is null) return BadRequest(new {error = $"Claim {cModel.claimType} does not exist!"});
            
            var customClaim = new Claim(cModel.claimType, cModel.value);            
            var result = await _userManager.RemoveClaimAsync(_user, customClaim);
            
            if (result.Succeeded) 
            {
                await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Claim Removed!", cModel});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> AddRole(string role)
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();
            var hasRole = User.IsInRole(role);
            if(hasRole) return BadRequest(new {error = $"User is already in role {role}"});
            var result = await _userManager.AddToRoleAsync(_user, role);
            if (result.Succeeded) 
            {
                await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Role added!", role});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> RemoveRole(string role)
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();
            var hasRole = User.IsInRole(role);
            if (!hasRole) return BadRequest(new{error = $"User does not have role {role}"});
            var result = await _userManager.RemoveFromRoleAsync(_user, role);
            if (result.Succeeded) 
            {
                await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Role removed!", role});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> ListClaims()
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();
            var listOfClaims = await _userManager.GetClaimsAsync(_user);
            return Ok(listOfClaims);
        }

        public async Task<IActionResult> ListRoles()
        {
            var _user = await _userManager.GetUserAsync(User);
            if(_user is null) return BadRequest();
            var listOfRoles = await _userManager.GetRolesAsync(_user);
            return Ok(listOfRoles);
        }

    }
}