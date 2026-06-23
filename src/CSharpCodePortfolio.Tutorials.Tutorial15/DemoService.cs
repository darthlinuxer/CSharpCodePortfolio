namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal sealed class DemoService : IDemoService
{
    public string ServiceDemo(int number) =>
        $"DemoService executou operação {number}";
}
