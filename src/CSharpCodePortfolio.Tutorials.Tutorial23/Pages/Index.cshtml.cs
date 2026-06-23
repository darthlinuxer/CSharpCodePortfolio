using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CSharpCodePortfolio.Tutorials.Tutorial23.Pages;

public sealed class IndexModel : PageModel
{
    public string Heading { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string RenderedAt { get; private set; } = string.Empty;

    public void OnGet()
    {
        Heading = "Razor Pages em .NET 10";
        Message = "A rota executa o PageModel e aplica o layout compartilhado.";
        RenderedAt = "tempo de requisição";
    }
}
