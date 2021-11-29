using System;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;

namespace MakingHttpRequests.Controllers;

[ApiController]
[Route("[controller]")]
public class BibleController : ControllerBase
{
    private readonly ILogger<BibleController> _logger;
    private readonly IHttpClientFactory factory;
    private readonly BibleService bibleService;

    public BibleController(
        ILogger<BibleController> logger,
        IHttpClientFactory factory,
        BibleService bibleService)
    {
        _logger = logger;
        this.factory = factory;
        this.bibleService = bibleService;
    }

    [HttpGet]
    [Route("Redirect/{book}/{chapter}/{verse}")]
    public IActionResult Get(string book, string chapter, string verse)
    {
        var URL = $"https://bible-api.com/{book} {chapter}:{verse}?translation=almeida";
        return Redirect(URL);
    }

    [HttpGet]
    [Route("[action]/{book}/{chapter}/{verse}")]
    public async Task<IActionResult> GetManual(string book, string chapter, string verse)
    {
        var URL = $"https://bible-api.com/{book} {chapter}:{verse}?translation=almeida";
        var httpClient = new HttpClient();
        var result = await httpClient.GetAsync(URL);
        return Ok(await result.Content.ReadAsStringAsync());
        //on console
        //netstat -ano | findstr 104.131.238.28
    }

    [HttpGet]
    [Route("[action]/{book}/{chapter}/{verse}")]
    public async Task<IActionResult> GetManualWithUsing(string book, string chapter, string verse)
    {
        var URL = $"https://bible-api.com/{book} {chapter}:{verse}?translation=almeida";
        using var httpClient = new HttpClient();
        var result = await httpClient.GetAsync(URL);
        return Ok(await result.Content.ReadAsStringAsync());
        //on console
        //netstat -ano | findstr 104.131.238.28
    }

    [HttpGet]
    [Route("[action]/{book}/{chapter}/{verse}")]
    public async Task<IActionResult> GetWithFactory(string book, string chapter, string verse)
    {
        var httpClient = factory.CreateClient("BibleClient");
        var URL = $"{httpClient.BaseAddress}/{book} {chapter}:{verse}?translation=almeida";

        var result = await httpClient.GetAsync(URL);
        return Ok(await result.Content.ReadAsStringAsync());
        //on console
        //netstat -ano | findstr 104.131.238.28
    }

    [HttpGet]
    [Route("[action]/{book}/{chapter}/{verse}")]
    public async Task<IActionResult> GetWithService(string book, string chapter, string verse)
    {
        return Ok(await bibleService.GetAsync(book, chapter, verse));
        //on console
        //netstat -ano | findstr 104.131.238.28
    }

    [HttpGet]
    [Route("[action]/{book}/{chapter}/{verse}")]
    public async Task<IActionResult> GetWithPolicy(string book, string chapter, string verse)
    {
        var httpClient = factory.CreateClient("BibleClient");
        var URL = $"{httpClient.BaseAddress}/{book} {chapter}:{verse}?translation=almeida";

        var policies = Policy.WrapAsync(
            Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)),
            Policy.Handle<HttpRequestException>().CircuitBreakerAsync(5, TimeSpan.FromSeconds(10))
        );

        return await policies.ExecuteAsync(async () =>
            {            
                var rand = new Random();
                if(rand.Next(1,3) == 1)
                    throw new HttpRequestException("Fake Request Exception");
                    
                var result = await httpClient.GetAsync(URL);
                var content = await result.Content.ReadAsStringAsync();
                return Ok(content);                
            }
        );
    }
}
