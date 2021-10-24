using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class OAuthUser: IdentityUser
    {
        [Required]
        public string Client_ID {get; set;}
        [Required]
        public string Client_Secret { get; set; }
        
    }
}