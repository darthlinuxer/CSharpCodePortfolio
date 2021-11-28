using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AsyncExceptionFilterAttribute>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new SyncActionFilterAttribute("Global")); 
    options.Filters.Add(new AsyncResourceFilterAttribute("Global")); 
    options.Filters.AddService<AsyncExceptionFilterAttribute>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run();
