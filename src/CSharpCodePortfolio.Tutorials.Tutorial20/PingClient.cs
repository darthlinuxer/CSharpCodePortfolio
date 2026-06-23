namespace CSharpCodePortfolio.Tutorials.Tutorial20;

internal sealed class PingClient(HttpClient httpClient)
{
    public async Task<PingResult> CheckAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("ping", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return new PingResult(response.StatusCode, body.Trim(), response.IsSuccessStatusCode);
    }
}
