using System.Net;
using System.Net.Http.Json;
using App;
using App.Context;
using App.Models;
using App.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class ControllerTest
{
    private WebApplicationFactory<Startup> _factory;
    private HttpClient httpClient;

    public ControllerTest()
    {
        _factory = new WebApplicationFactory<Startup>()
            .WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {

                    });
                });
        this.httpClient = _factory.CreateClient();
    }

    public async Task RegisterUser(UserRegisterData user)
    {
        await httpClient.PostAsJsonAsync<UserRegisterData>("OpenId/Register", user);
    }

    public async Task<string> LoginAndReturnToken(UserRegisterData user)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync<UserRegisterData>("OpenId/LoginAndReturnToken", user);
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
        var result = await httpClient.SendAsync(req);

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
        var result = await httpClient.SendAsync(req);

        result.Should().Be200Ok();

        ReturnedAccessToken? aToken = await result.Content.ReadFromJsonAsync<ReturnedAccessToken>();

        Assert.IsNotNull(aToken?.Access_Token);

        HttpRequestMessage req2 = new(HttpMethod.Get, $"OpenId/ValidateAccessTokenFromQuery?access_token={aToken?.Access_Token}");
        result = await httpClient.SendAsync(req2);
        Assert.IsTrue(await result.Content.ReadAsStringAsync() == "true");
    }

    [TestMethod]
    [DataRow("Hello World!")]
    public async Task LocalizationTest_ReturnHelloWorld(string key)
    {
        var response = await httpClient.GetAsync($"Localization/Test?key={key}");
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
        var scope = _factory.Services.CreateScope();
        ILocalizationService locService = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        ILanguageService langService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

        await httpClient.GetAsync($"Localization/SetCulture?culture={culture}");
        var response = await httpClient.GetAsync($"Localization/Test?key={key}");
        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ReturnedValue>();

        int id = langService.GetLanguageByCulture(culture).Id;
        string translation = locService.GetStringResource(key, id).Value;

        Assert.IsTrue(result?.Value == translation);
    }

}

public class ReturnedValue { public string? Value { get; set; } };
public class ReturnedToken { public string? Id_Token { get; set; } }
public class ReturnedAccessToken { public string? Access_Token { get; set; } }