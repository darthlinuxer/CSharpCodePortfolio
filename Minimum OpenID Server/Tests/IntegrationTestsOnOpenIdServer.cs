using System.Net.Http.Json;
using OpenIDAppMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class IntegrationTestsOnOpenIdServer
{
    private readonly WebApplicationFactory<OpenIDAppMVC.Startup> openIDfactory = new();
    private readonly HttpClient client;
    public IntegrationTestsOnOpenIdServer()
    {
        this.client = openIDfactory.CreateClient();
        openIDfactory.Server.BaseAddress = new Uri("https://localhost:5003");
    }

    public async Task<HttpResponseMessage> RegisterUser(UserRegisterData user)
    {
        return await client.PostAsJsonAsync("https://localhost:5003/User/Register",user);
    }
    public async Task<HttpResponseMessage> LoginAndReturnToken(UserRegisterData user)
    {
        return await client.PostAsJsonAsync("https://localhost:5001/User/LoginAndReturnToken",user);
    }

    //Test on OpenIDServer
    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task UserController_Register(string email, string password)
    {
        var response = await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var result = await response.Content.ReadFromJsonAsync<IdentityUser>();
        Assert.IsTrue(result?.Email == email);
    }

    //Test on MainApp
    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task UserController_LoginAndReturnToken_TokenMustBeValid(string email, string password)
    {
        await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var response = await LoginAndReturnToken(new UserRegisterData{Login=email, Password = password});
        var token = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(token);

        HttpRequestMessage req = new();
        req.Headers.Add("Authorization","Bearer "+token);
        req.RequestUri = new Uri("https://localhost:5003/Validation/TestWithIdToken");
        req.Method = HttpMethod.Get;
        response = await client.SendAsync(req);
        ReturnMsg? Msg = await response.Content.ReadFromJsonAsync<ReturnMsg>();
        Assert.IsTrue(Msg?.Msg == "You are authorized to read this endpoint with Id Token!");
    }

     //Test on MainApp
    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task UserController_RequestAccessToken_TokenMustBeValid(string email, string password)
    {
        await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var response = await LoginAndReturnToken(new UserRegisterData{Login=email, Password = password});
        var id_token = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(id_token);

        HttpRequestMessage req = new();
        req.Headers.Add("Authorization","Bearer "+id_token);
        req.RequestUri = new Uri("https://localhost:5003/User/RequestAccessToken");
        req.Method = HttpMethod.Get;
        response = await client.SendAsync(req);
        string? access_token = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(access_token);

        req = new HttpRequestMessage();
        req.Headers.Add("Access-Token",access_token);
        req.RequestUri = new Uri("https://localhost:5003/Validation/ValidateAccessTokenFromAttributes");
        req.Method = HttpMethod.Get;
        response = await client.SendAsync(req);
        IdentityUser? user = await response.Content.ReadFromJsonAsync<IdentityUser>();

        Assert.IsTrue(user?.Email == email);
    }

    
}