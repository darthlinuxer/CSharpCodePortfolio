using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using OpenIDApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OpenIDApp.Controllers
{
    public class LocalizationController : ControllerBase
    {
        private readonly IOptions<RequestLocalizationOptions> locOptions;
        private readonly ILocalizationService localizationService;
        private readonly ILanguageService languageService;

        public LocalizationController(
            IOptions<RequestLocalizationOptions> LocOptions,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            locOptions = LocOptions;
            this.localizationService = localizationService;
            this.languageService = languageService;
        }

        public IActionResult Test([FromQuery] string key)
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            var language = languageService.GetLanguageByCulture(currentCulture);
            if (language is null) return Ok(new{value = key});
            var stringResource = localizationService.GetStringResource(key, language.Id);
            if (stringResource is null) return Ok(new{value = key});
            return Ok(new{value = stringResource.Value});
        }

        public IActionResult GetSupportedCultures()
        {
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
            var cultureItems = locOptions.Value.SupportedCultures
                .Select(c => new { c.Name, c.DisplayName, c.NativeName })
                .ToList();

            return Ok(new { cultureItems, requestCulture.RequestCulture });
        }

        public IActionResult GetCultureProviders()
        {
            return Ok(locOptions.Value.RequestCultureProviders);
        }

        [HttpGet]
        public IActionResult SetCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)
                ),
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );
            return Ok(new { msg = "Cookie language set to " + culture });
        }

        public IActionResult GetCurrentCulture()
        {
            // Retrieves the requested culture
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            // Culture contains the information of the requested culture
            var culture = rqf.RequestCulture.Culture;
            return Ok(new {culture, Thread.CurrentThread.CurrentCulture});
        }



    }
}