using System.Net.Http.Json;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class IntegrationTestsOnMainApp
{
    private readonly WebApplicationFactory<App.Startup> mainAppFactory = new();
    private readonly HttpClient client;
    public IntegrationTestsOnMainApp()
    {
        this.client = mainAppFactory.CreateClient();
        mainAppFactory.Server.BaseAddress = new Uri("https://localhost:5001");
    }

    public async Task<HttpResponseMessage> RegisterUser(UserRegisterData user)
    {
        return await client.PostAsJsonAsync("https://localhost:5001/AccessControl/RegisterUser",user);
    }
    public async Task<HttpResponseMessage> LoginAndReturnToken(UserRegisterData user)
    {
        return await client.PostAsJsonAsync("https://localhost:5001/AccessControl/LoginAndReturnToken",user);
    }

    //Test on MainApp
    [TestMethod]
    [DataRow("Hello World!")]
    [DataRow("Olá Mundo!")]
    public async Task AccessController_Test_MustReturnMessage(string msg)
    {
        var result = await client.GetAsync($"https://localhost:5001/AccessControl/Test?msg={msg}");
        Assert.IsTrue(msg == await result.Content.ReadAsStringAsync());
    }

    //Test on MainApp
    [TestMethod]
    [DataRow("chaves.camilo@gmail.com","123")]
    public async Task AccessController_RegisterUser(string email, string password)
    {
        var response = await RegisterUser(new UserRegisterData{Login=email,Password=password});
        var result = await response.Content.ReadFromJsonAsync<IdentityUser>();
        Assert.IsTrue(result?.Email == email);
    }

    //Test on MainApp
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
        IdentityUser? user = await response.Content.ReadFromJsonAsync<IdentityUser>();
        Assert.IsTrue(user?.Email == email);
    }

    
}