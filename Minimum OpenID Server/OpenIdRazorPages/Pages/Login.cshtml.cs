using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIDAppRazor.Models;

namespace OpenIDAppRazor.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public UserRegisterData UserRegisterData {get; set;}
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost(string email, string password)
        {
            if (!ModelState.IsValid) return Page();

            return new JsonResult(
                new
                {
                    _email = Request.Form["email"],
                    _password = Request.Form["password"],
                    email, password,
                    this.UserRegisterData
                });
        }
    }
}
