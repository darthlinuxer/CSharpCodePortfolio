using System.Net;
using System.Net.Http.Json;
using OpenIDApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Models;

namespace Tests;

[TestClass]
public class OAuthTests
{
    private WebApplicationFactory<OpenIDApp.Startup> _openIDfactory;
    private WebApplicationFactory<OAuthApp.Startup> _oauthfactory;
    private HttpClient _openIdHttpClient;
    private HttpClient _oauthHttpClient;

    public OAuthTests()
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

    public async Task<ReturnedMsgAndUser> RegisterUser(UserRegisterData user)
    {
        HttpResponseMessage response = await _oauthHttpClient.PostAsJsonAsync<UserRegisterData>(
            "https://localhost:5003/AccessControl/RegisterUser", user);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content?.ReadFromJsonAsync<ReturnedMsgAndUser>();
        } else
        return null;
    }

    public async Task<string> LoginAndReturnToken(UserRegisterData user)
    {
        HttpResponseMessage response = await _oauthHttpClient.PostAsJsonAsync<UserRegisterData>(
            "https://localhost:5003/AccessControl/LoginAndReturnToken", user);
        
        ReturnedToken? result = null;
        if (response.StatusCode == HttpStatusCode.OK) result = await response.Content.ReadFromJsonAsync<ReturnedToken>();
        return result?.Id_Token ?? "";
    }

    [TestMethod]
    public async Task BasicController_Index_MustReturnHelloWorld()
    {
        var response = await _oauthHttpClient.GetAsync("https://localhost:5003/Basic/Index");
        ReturnedMessage? Msg = await response.Content.ReadFromJsonAsync<ReturnedMessage>();
        Assert.AreEqual(Msg?.Msg, "Hello World!");
        response.Should().Be200Ok();
    }

    [TestMethod]
    public async Task AccessController_LoginAndReturnToken_MustReturnToken()
    {
        var user = new UserRegisterData{Login = "camilo@chaves.com",Password="123"};
        await RegisterUser(user);
        var token = await LoginAndReturnToken(user);
        token.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task AccessControl_Test_MustReturnTheQueryMsg()
    {
        var msg = await _oauthHttpClient.GetStringAsync("https://localhost:5003/AccessControl/Test?msg=test");
        Assert.AreEqual(msg,"test");
    }

    [TestMethod]
    [DataRow("Google")]
    public async Task AccessControl_LoginWithProvider(string provider)
    {
        var response = await _oauthHttpClient.GetAsync($"https://localhost:5003/AccessControl/LoginWithProvider/{provider}");
        if (response.IsSuccessStatusCode)
        {

        }
    }

  
}

