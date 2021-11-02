using System.Net;
using System.Net.Http.Json;
using OpenIDApp.Models;
using OpenIDApp.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Models;

namespace Tests;

[TestClass]
public class OpenIDTest
{
    private WebApplicationFactory<OpenIDApp.Startup> _openIDfactory;
    private WebApplicationFactory<OAuthApp.Startup> _oauthfactory;
    private HttpClient _openIdHttpClient;
    private HttpClient _oauthHttpClient;

    public OpenIDTest()
    {
        _openIDfactory = new WebApplicationFactory<OpenIDApp.Startup>()
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {

                    });
                });
        _oauthfactory = new WebApplicationFactory<OAuthApp.Startup>();
        _openIDfactory.Server.BaseAddress = new Uri("https://localhost:5001");
        _oauthfactory.Server.BaseAddress = new Uri("https://localhost:5003");

        this._openIdHttpClient = _openIDfactory.CreateClient();
        this._oauthHttpClient = _oauthfactory.CreateClient();
    }

    public async Task RegisterUser(UserRegisterData user)
    {
        await _openIdHttpClient.PostAsJsonAsync<UserRegisterData>("OpenId/Register", user);
    }

    public async Task<string> LoginAndReturnToken(UserRegisterData user)
    {
        HttpResponseMessage response = await _openIdHttpClient.PostAsJsonAsync<UserRegisterData>("OpenId/LoginAndReturnToken", user);
        ReturnedToken? result = await response.Content.ReadFromJsonAsync<ReturnedToken>();
        return result?.Id_Token ?? "";
    }

    [TestMethod]
    public async Task OpenIDController_ValidateIdToken()
    {
        var user = new UserRegisterData() { Login = "camilo@chaves.com", Password = "123" };
        await RegisterUser(user);
        string token = await LoginAndReturnToken(user);

        HttpRequestMessage req = new(HttpMethod.Get, "OpenId/WhoAmIFromToken");
        req.Headers.Add("Authorization", "Bearer " + token);
        // httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        //     "Bearer",
        //     token);
        var result = await _openIdHttpClient.SendAsync(req);

        IdentityUser? loggedUser = await result.Content.ReadFromJsonAsync<IdentityUser>();
        Assert.IsTrue(loggedUser?.Email == user.Login);
    }

    [TestMethod]
    public async Task OpenIDController_ValidateAccessToken()
    {
        var user = new UserRegisterData() { Login = "camilo@chaves.com", Password = "123" };
        await RegisterUser(user);
        string token = await LoginAndReturnToken(user);

        Assert.IsNotNull(token);

        HttpRequestMessage req = new(HttpMethod.Get, "OpenId/RequestAccessToken");
        req.Headers.Add("Authorization", "Bearer " + token);
        var result = await _openIdHttpClient.SendAsync(req);

        result.Should().Be200Ok();

        ReturnedAccessToken? aToken = await result.Content.ReadFromJsonAsync<ReturnedAccessToken>();

        Assert.IsNotNull(aToken?.Access_Token);

        HttpRequestMessage req2 = new(HttpMethod.Get, $"OpenId/ValidateAccessTokenFromQuery?access_token={aToken?.Access_Token}");
        result = await _openIdHttpClient.SendAsync(req2);
        Assert.IsTrue(await result.Content.ReadAsStringAsync() == "true");
    }

    [TestMethod]
    [DataRow("Hello World!")]
    public async Task LocalizationTest_ReturnHelloWorld(string key)
    {
        var response = await _openIdHttpClient.GetAsync($"Localization/Test?key={key}");
        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ReturnedValue>();
        Assert.IsTrue(result?.Value == "Olá Mundo!");
    }

    [TestMethod]
    [DataRow("fr-FR", "Hello World!")]
    [DataRow("de-DE", "Hello World!")]
    [DataRow("pt-BR", "Hello World!")]
    public async Task LocalizationTest_SetLanguageToFrenchAndReturnTranslation(string culture, string key)
    {
        var scope = _openIDfactory.Services.CreateScope();
        ILocalizationService locService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        ILanguageService langService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

        await _openIdHttpClient.GetAsync($"Localization/SetCulture?culture={culture}");
        var response = await _openIdHttpClient.GetAsync($"Localization/Test?key={key}");
        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ReturnedValue>();

        int id = langService.GetLanguageByCulture(culture).Id;
        string translation = locService.GetStringResource(key, id).Value;

        Assert.IsTrue(result?.Value == translation);
    }

}
