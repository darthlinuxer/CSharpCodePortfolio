using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpenIDAppRazor.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; set; }
        public void OnGet([FromQuery] int id)
        {
            @ViewData["Hello"] = "Hello World!";
            Message = "Get used with id = "+id;
        }
        public void OnPost(int id)
        {
            Message = "Post used. Id = "+id;
        }

        public void OnPostPut([FromRoute] int id)
        {
            Message = "Put used: Id = "+id;
        }
        public void OnPostDelete(int id)
        {
            Message = "Delete used: Id = "+id;
        }
        public void OnPostSubmit(string msg)
        {
            Message = "Message Submited = " + msg;
        }
    }

}


