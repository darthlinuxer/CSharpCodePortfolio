using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace OpenIDApp.Middlewares
{
    public class RequestLocalizationCookiesMiddleware : IMiddleware
    {
        public CookieRequestCultureProvider Provider {get;}
        public RequestLocalizationCookiesMiddleware(
            IOptions<RequestLocalizationOptions> requestLocOptions
        )
        {
            Provider = requestLocOptions
                            .Value
                            .RequestCultureProviders
                            .Where(x=>x is CookieRequestCultureProvider)
                            .Cast<CookieRequestCultureProvider>()
                            .FirstOrDefault();            
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if(Provider is null) await next(context);
            var feature = context.Features.Get<IRequestCultureFeature>();
            if(feature is null) await next(context);
            context.Response.Cookies.Append(
                Provider.CookieName,
                CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture)
            );
            await next(context);
        }
    }
}