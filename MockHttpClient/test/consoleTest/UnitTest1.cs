using System.Text;
using ConsoleNetwork;
using Moq;
using Moq.Protected;

[TestClass]
public class UnitTest1
{
    public NetworkConnect? testConnection;
    [TestInitialize]
    public void Setup()
    {


    }

    [TestMethod]
    public async Task TestMethod1()
    {
        //arrange
        Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

        HttpResponseMessage httpResponse = new HttpResponseMessage();
        httpResponse.StatusCode = System.Net.HttpStatusCode.OK;
        httpResponse.Content = new StringContent("hahaha", Encoding.UTF8, "application/json");

         var uri = new Uri("http://127.0.0.1:5244/ping");

        mockHandler.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync",
           ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri.ToString().StartsWith(uri.ToString())),
           ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(httpResponse)
           .Verifiable();

        testConnection = new()
        {
            Client = new HttpClient(mockHandler.Object)
        };


        //Act
        await testConnection.TestPing();

        //Assert
        mockHandler.Protected().Verify(
            "SendAsync", // Method name
            Times.AtLeastOnce(), // At least once
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
   );

    }
}