using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("BibleClient", c =>
{
    c.BaseAddress = new Uri("https://bible-api.com/");
    //c.Timeout = System.TimeSpan.FromSeconds(2);
});

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
builder.Services.AddHttpClient<BibleService>(c =>
{
    c.BaseAddress = new Uri("https://bible-api.com/");
})
.AddTransientHttpErrorPolicy(
    builder => builder.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(3)))
.AddTransientHttpErrorPolicy(
    builder => builder.CircuitBreakerAsync(5, _ = TimeSpan.FromSeconds(10))
)
.AddPolicyHandler(request =>
{
    if(request.Method == HttpMethod.Get) return timeoutPolicy;

    return Policy.NoOpAsync<HttpResponseMessage>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.MapControllers();

app.Run();
