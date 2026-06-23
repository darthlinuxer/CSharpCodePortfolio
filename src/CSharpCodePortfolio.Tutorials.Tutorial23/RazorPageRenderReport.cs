using System.Net;

namespace CSharpCodePortfolio.Tutorials.Tutorial23;

internal sealed record RazorPageRenderReport(
    string BaseAddress,
    HttpStatusCode StatusCode,
    HttpStatusCode PrivacyStatusCode,
    string Title,
    bool HasLayout,
    bool HasNavigation,
    bool HasPageModelContent,
    bool HasRazorPageMarker);
