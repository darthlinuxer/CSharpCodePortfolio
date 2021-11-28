using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ConsoleLoggerMiddleWare>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/favicon.ico", (context) => Task.CompletedTask);
app.UseMiddleware<ConsoleLoggerMiddleWare>();

app.Use(async (context, next) =>
{
    Console.WriteLine("Before Request: Inline Middleware!");
    await next();
    Console.WriteLine("After Request: Inline Middleware!");
});

app.UseWhen(context => context.Request.Query["key"].Any(), app =>
{
    Console.WriteLine("Use When Pipeline reached!");
    app.Run(async context => await context.Response.WriteAsync("Use When Pipeline reached!"));
});

app.MapGet("/", async (context) =>
{
    Console.WriteLine("Hello World Endpoint!");
    await context.Response.WriteAsync("Hello World Endpoint reached!");    
});



//Terminal Middlewares
// app.Run(async context =>
// {
//     Console.WriteLine("Terminal Middleware!");
//     //await context.Response.WriteAsync("Terminal Middleware");
// });
// app.Run(async context =>
//  {
//      Console.WriteLine("Never Reached Middleware!");
//  });

app.Run();