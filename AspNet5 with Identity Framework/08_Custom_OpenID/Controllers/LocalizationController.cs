using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace App.Controllers
{
    public class LocalizationController : ControllerBase
    {
        private readonly IOptions<RequestLocalizationOptions> locOptions;

        public LocalizationController(
            IOptions<RequestLocalizationOptions> LocOptions
        )
        {
            locOptions = LocOptions;
        }

        public IActionResult GetLanguages()
        {
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
            var cultureItems = locOptions.Value.SupportedUICultures
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Name, Text = c.DisplayName })
                .ToList();
            var returnUrl =
                string.IsNullOrEmpty(HttpContext.Request.Path) ?
                "~/" :
                $"~{HttpContext.Request.Path.Value}";
            return Ok(returnUrl);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );
            return Ok(new {msg = "Cookie language set to "+culture});
        }



    }
}