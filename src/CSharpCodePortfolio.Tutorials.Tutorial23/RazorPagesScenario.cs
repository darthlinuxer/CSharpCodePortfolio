using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpCodePortfolio.Tutorials.Tutorial23;

internal sealed class RazorPagesScenario
{
    public async Task<RazorPageRenderReport> RunAsync(CancellationToken cancellationToken)
    {
        await using var app = await CreateAppAsync(cancellationToken);

        try
        {
            var address = GetAddress(app);
            using var httpClient = new HttpClient { BaseAddress = new Uri(address) };

            using var indexResponse = await httpClient.GetAsync("/", cancellationToken);
            var indexHtml = await indexResponse.Content.ReadAsStringAsync(cancellationToken);
            using var privacyResponse = await httpClient.GetAsync("/Privacy", cancellationToken);

            return new RazorPageRenderReport(
                address,
                indexResponse.StatusCode,
                privacyResponse.StatusCode,
                ReadTitle(indexHtml),
                indexHtml.Contains("data-layout=\"portfolio\"", StringComparison.Ordinal),
                indexHtml.Contains("href=\"/Privacy\"", StringComparison.Ordinal),
                indexHtml.Contains("Renderizado pelo PageModel", StringComparison.Ordinal),
                indexHtml.Contains("data-page=\"index\"", StringComparison.Ordinal));
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }

    internal static async Task<WebApplication> CreateAppAsync(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services
            .AddRazorPages()
            .AddApplicationPart(typeof(RazorPagesScenario).Assembly);

        var app = builder.Build();
        app.MapRazorPages();

        await app.StartAsync(cancellationToken);
        return app;
    }

    private static string GetAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()
            ?.Addresses;

        return addresses?.Single()
            ?? throw new InvalidOperationException("O servidor Razor Pages deve expor um endereço local.");
    }

    private static string ReadTitle(string html)
    {
        var match = Regex.Match(html, "<title>(?<title>.*?)</title>", RegexOptions.Singleline);
        return WebUtility.HtmlDecode(match.Groups["title"].Value.Trim());
    }
}
