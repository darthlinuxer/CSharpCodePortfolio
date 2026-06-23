namespace CSharpCodePortfolio.Tutorials.Tutorial15;

internal sealed class TestService(IDemoService demoService) : ITestService
{
    public IReadOnlyList<string> RunDemo() =>
    [
        demoService.ServiceDemo(0),
        demoService.ServiceDemo(1)
    ];
}
