using Microsoft.AspNetCore.Mvc;

namespace OpenIDAppMVC.Controllers
{
    public class HomeController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}