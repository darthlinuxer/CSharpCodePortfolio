public class BibleService
{
    private readonly HttpClient httpClient;

    public BibleService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetAsync(string book, string chapter, string verse){
        string URL = $"{httpClient.BaseAddress}/{book} {chapter}:{verse}?translation=almeida";
        var result = await httpClient.GetAsync(URL);

        return await result.Content.ReadAsStringAsync();
    }
}