using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;
configuration.AddJsonFile("MyConfig.json", optional:true, reloadOnChange: true);

var inMemory = new Dictionary<string,string>
{
    {"MemoryKey","Hello From Memory"}
};
configuration.AddInMemoryCollection(inMemory);

builder.Services.AddControllers();

//builder.Services.Configure<MyApiOptions>(configuration.GetSection("MyApi"));
builder.Services.AddOptions<MyApiOptions>()
                .Bind(configuration.GetSection("MyApi"))
                .ValidateDataAnnotations();

var app = builder.Build();

IConfiguration config = app.Configuration;
IWebHostEnvironment env = app.Environment;

Console.WriteLine($"MyKey: {config["MyKey"]} ");
Console.WriteLine($"MyKey1: {config["MyKey1"]} ");

//dotnet run CommandLine="FromCommandLine"
Console.WriteLine($"CommandLine: {config["CommandLine"]} "); 

Console.WriteLine($"Path: {Environment.GetEnvironmentVariable("Path")} "); 

//Getting complex configurations

Console.WriteLine($"MyApi-Url: {config["MyApi:Url"]}");
Console.WriteLine($"MyApi-Key: {config["MyApi:Key"]}");

var apiOptions = new MyApiOptions();
config.GetSection("MyApi").Bind(apiOptions);
Console.WriteLine($"MyApi-Url Bind: {apiOptions.URL}");
Console.WriteLine($"MyApi-Key Bind: {apiOptions.Key}");

var anotherWay = config.GetSection("MyApi").Get<MyApiOptions>();
Console.WriteLine($"MyApi-Url Get: {anotherWay.URL}");
Console.WriteLine($"MyApi-Key Get: {anotherWay.Key}");

//Now on the CLI
//dotnet user-secrets init
//dotnet user-secrets set "MyApi:OtherKey" "Sensitive Info from Secrets"
//dotnet user-secrets list
//dotnet user-secrets remove "MyApi:OtherKey" 
Console.WriteLine($"MyApi-OtherKey From Secrets: {config["MyApi:OtherKey"]}");

//Configuration from another file
Console.WriteLine($"MyApi-Key From Custom Config: {config["MyKeyFromMyConfig"]}");

//From Memory
Console.WriteLine($"MyApi-Key From Memory: {config["MemoryKey"]}");

//Default Values if Not Found on any of Config sources
Console.WriteLine($"TryFindKey: {config.GetValue<int>("TryFindKey",10)}");

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run();
