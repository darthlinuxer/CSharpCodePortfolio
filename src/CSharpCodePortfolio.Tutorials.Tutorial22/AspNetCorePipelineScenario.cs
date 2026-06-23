using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharpCodePortfolio.Tutorials.Tutorial22;

internal sealed class AspNetCorePipelineScenario
{
    public async Task<AspNetCorePipelineReport> RunAsync(CancellationToken cancellationToken)
    {
        var builder = CreateBuilder();
        await using var app = builder.Build();
        ConfigurePipeline(app);

        var options = app.Services.GetRequiredService<IOptions<PortfolioApiOptions>>().Value;
        await app.StartAsync(cancellationToken);

        try
        {
            var address = GetAddress(app);
            var trace = app.Services.GetRequiredService<PipelineTrace>();
            using var httpClient = new HttpClient { BaseAddress = new Uri(address) };

            var employeeRequest = await SendAsync(httpClient, "/employees/7", trace, cancellationToken);
            var cachedRequest = await SendAsync(httpClient, "/employees/7?cache=true", trace, cancellationToken);
            var metadata = ReadEmployeeEndpointMetadata(app);

            return new AspNetCorePipelineReport(
                address,
                options.ApiUrl,
                options.ApiKey,
                options.RetryCount,
                metadata.Name,
                metadata.Tags,
                employeeRequest,
                cachedRequest);
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }

    internal static WebApplicationBuilder CreateBuilder()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PortfolioApi:ApiUrl"] = "https://portfolio.local/verses/",
            ["PortfolioApi:ApiKey"] = "local-key",
            ["PortfolioApi:RetryCount"] = "3"
        });

        builder.Services.AddSingleton<PipelineTrace>();
        builder.Services.AddTransient<RequestTraceMiddleware>();
        builder.Services.AddSingleton<EmployeeDirectory>();
        builder.Services.AddTransient<EmployeeQueryHandler>();
        builder.Services
            .AddOptions<PortfolioApiOptions>()
            .Bind(builder.Configuration.GetSection("PortfolioApi"))
            .Validate(static options => options.ApiUrl.StartsWith("https://", StringComparison.Ordinal), "A URL da API deve usar HTTPS.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.ApiKey), "A chave da API deve ser informada.")
            .Validate(static options => options.RetryCount > 0, "O número de tentativas deve ser positivo.")
            .ValidateOnStart();
        builder.Services
            .AddHttpClient<VerseClient>()
            .ConfigurePrimaryHttpMessageHandler(static services =>
                new LocalVerseMessageHandler(services.GetRequiredService<PipelineTrace>()));

        return builder;
    }

    internal static void ConfigurePipeline(WebApplication app)
    {
        var trace = app.Services.GetRequiredService<PipelineTrace>();

        app.UseMiddleware<RequestTraceMiddleware>();
        app.Use(async (context, next) =>
        {
            trace.Record("middleware:inline:antes");
            await next(context);
            trace.Record("middleware:inline:depois");
        });
        app.UseWhen(
            static context => context.Request.Query.ContainsKey("cache"),
            branch => branch.Run(async context =>
            {
                trace.Record("ramificacao:cache");
                await context.Response.WriteAsJsonAsync(
                    new EmployeeResponse(0, "Resposta em cache", "sem consulta", "Resposta entregue pela ramificação."));
            }));

        app.MapGet("/employees/{id:int}", GetEmployeeAsync)
            .AddEndpointFilter<EndpointTraceFilter>()
            .WithName("GetPortfolioEmployee")
            .WithTags("Employees")
            .Produces<EmployeeResponse>();
    }

    internal static async Task<IResult> GetEmployeeAsync(
        int id,
        EmployeeQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new GetEmployeeQuery(id), cancellationToken);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<HttpExchangeReport> SendAsync(
        HttpClient httpClient,
        string path,
        PipelineTrace trace,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeResponse>(cancellationToken);
        return new HttpExchangeReport(response.StatusCode, employee, trace.SnapshotAndClear());
    }

    private static string GetAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()
            ?.Addresses;

        return addresses?.Single()
            ?? throw new InvalidOperationException("O servidor ASP.NET Core deve expor um endereço local.");
    }

    private static EndpointMetadataReport ReadEmployeeEndpointMetadata(WebApplication app)
    {
        var endpoint = app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .Single(static endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "GetPortfolioEmployee");
        var tags = endpoint.Metadata.GetMetadata<ITagsMetadata>()?.Tags ?? [];

        return new EndpointMetadataReport("GetPortfolioEmployee", tags.ToArray());
    }
}
