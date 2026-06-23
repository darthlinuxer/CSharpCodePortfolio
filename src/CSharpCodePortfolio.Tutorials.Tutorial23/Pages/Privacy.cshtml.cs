using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CSharpCodePortfolio.Tutorials.Tutorial23.Pages;

public sealed class PrivacyModel : PageModel
{
    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public void OnGet()
    {
        Title = "Privacidade";
        Message = "Roteamento de Razor Pages encontra arquivos pela pasta Pages.";
    }
}
