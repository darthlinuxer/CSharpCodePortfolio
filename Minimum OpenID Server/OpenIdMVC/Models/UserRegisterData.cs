using System.ComponentModel.DataAnnotations;

namespace OpenIDAppMVC.Models
{
    public class UserRegisterData
    {
        [Required(ErrorMessage = "Well, this is obviously required!")]
        [MinLength(3, ErrorMessage = "Login has to be largen than 3")]
        public string Login {get; set;}

        [Required]
        [MinLength(5, ErrorMessage = "Password has to be larger than 5")]
        public string Password {get; set;}
    }

}