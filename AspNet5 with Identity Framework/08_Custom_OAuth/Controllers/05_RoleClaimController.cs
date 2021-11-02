using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace OAuthApp.Controllers
{
    [Authorize(Policy = "Bearer")]
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
        public async Task<IActionResult> AddClaim([FromBody] ClaimModel cModel, bool updateCookie = false)
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest();

            var hasClaim = User.HasClaim(c=>c.Type == cModel.claimType);
            if(hasClaim) return BadRequest(new {error = $"Claim {cModel.claimType} already exist!"});
            
            var customClaim = new Claim(cModel.claimType, cModel.value);
            var result = await _userManager.AddClaimAsync(_user, customClaim);
            if (result.Succeeded) 
            {                
                if(updateCookie) await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Claim Added!", cModel});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> RemoveClaim([FromBody] ClaimModel cModel, bool updateCookie = false)
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest();

            IList<Claim> userClaims = await _userManager.GetClaimsAsync(_user);
            var hasClaim = userClaims.FirstOrDefault(x=>x.Type == cModel.claimType);
            if(hasClaim is null) return BadRequest(new {error = $"Claim {cModel.claimType} does not exist!"});
            
            var customClaim = new Claim(cModel.claimType, cModel.value);            
            var result = await _userManager.RemoveClaimAsync(_user, customClaim);
            
            if (result.Succeeded) 
            {
                if(updateCookie)await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Claim Removed!", cModel});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> AddRole(string role, bool updateCookie = false)
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest();
            var hasRole = User.IsInRole(role);
            if(hasRole) return BadRequest(new {error = $"User is already in role {role}"});
            var result = await _userManager.AddToRoleAsync(_user, role);
            if (result.Succeeded) 
            {
                if(updateCookie) await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Role added!", role});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> RemoveRole(string role, bool updateCookie=false)
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest();
            var hasRole = User.IsInRole(role);
            if (!hasRole) return BadRequest(new{error = $"User does not have role {role}"});
            var result = await _userManager.RemoveFromRoleAsync(_user, role);
            if (result.Succeeded) 
            {
                if(updateCookie)await _signInManager.SignInAsync(_user,false);
                return Ok(new {msg = "Role removed!", role});
            }
            return BadRequest(new {result.Errors});
        }

        public async Task<IActionResult> ListClaims()
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest(new {user = User.Identity.Name, error = "Not Found!"});
            var listOfClaims = await _userManager.GetClaimsAsync(_user);
            return Ok(listOfClaims);
        }

        public async Task<IActionResult> ListRoles()
        {
            var _user = await _userManager.FindByIdAsync(User.Identity.Name);
            if(_user is null) return BadRequest();
            var listOfRoles = await _userManager.GetRolesAsync(_user);
            return Ok(listOfRoles);
        }

    }
}