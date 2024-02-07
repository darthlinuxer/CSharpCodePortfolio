namespace ConsoleNetwork;
public class NetworkConnect
{
    public HttpClient? Client { get; set; }

    public async Task TestPing()
    {
        if (Client is null) Client = new HttpClient();
        var uri = new Uri("http://127.0.0.1:5244/ping");
        Console.WriteLine($"Trying to access: {uri}");
        var result = await Client.GetAsync(uri);
        var statusCode = result.StatusCode;
        var content = await result.Content.ReadAsStringAsync();
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Content: {content}");
    }

}