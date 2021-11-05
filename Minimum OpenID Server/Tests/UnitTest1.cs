using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIDApp.Models;
using Org.BouncyCastle.Tsp;

namespace Tests;

[TestClass]
public class UnitTest1
{
    private WebApplicationFactory<App.Startup> mainFactory = new();
    private WebApplicationFactory<OpenIDApp.Startup> openIDfactory = new();
    private HttpClient client;
    public UnitTest1()
    {
        this.client = mainFactory.CreateClient();
        mainFactory.Server.BaseAddress = new Uri("https://localhost:5001");
        openIDfactory.Server.BaseAddress = new Uri("https://localhost:5002");
    }

    public async Task<HttpResponseMessage> RegisterUser(UserRegisterData user)
    {
        return await client.PostAsJsonAsync<UserRegisterData>("https://localhost:5001/AccessControl/RegisterUser",user);
    }
    public async Task<HttpResponseMessage> LoginAndReturnToken(UserRegisterData user)
    {
        return await client.PostAsJsonAsync<UserRegisterData>("https://localhost:5001/AccessControl/LoginAndReturnToken",user);
    }

    [TestMethod]
    [DataRow("Hello World!")]
    [DataRow("Olá Mundo!")]
    public async Task AccessController_Test_MustReturnMessage(string msg)
    {
        var result = await client.GetAsync($"https://localhost:5001/AccessControl/Test?msg={msg}");
        Assert.IsTrue(msg == await result.Content.ReadAsStringAsync());
    }

    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task AccessController_RegisterUser(string email, string password)
    {
        var response = await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var result = await response.Content.ReadFromJsonAsync<IdentityUser>();
        Assert.IsTrue(result?.Email == email);
    }

    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task AccessController_LoginAndReturnToken_TokenMustBeValid(string email, string password)
    {
        await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var response = await LoginAndReturnToken(new UserRegisterData{Login=email, Password = password});
        var token = await response.Content.ReadAsStringAsync();
        Assert.IsNotNull(token);

        HttpRequestMessage req = new();
        req.Headers.Add("Authorization","Bearer "+token);
        req.RequestUri = new Uri("https://localhost:5001/AccessControl/WhoAmIFromToken");
        req.Method = HttpMethod.Get;
        response = await client.SendAsync(req);
        string? user = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(user == email);
    }

    
}