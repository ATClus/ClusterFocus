using Hub.Application;
using Hub.Infrastructure.Repositories;
using Hub.Infrastructure;
using Hub.Presentation;
using Microsoft.EntityFrameworkCore;
using Hub.Infrastructure.Services;
using Serilog;

// Add Log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddDbContext<HubDbContext>(options =>
    options.UseSqlite("Data Source=hub.db"));
builder.Services.AddScoped<ITabRepository, TabRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddCors(options =>
{    
    options.AddPolicy("BrowserPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => origin.StartsWith("moz-extension://"))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
            });
});
builder.Services.AddHostedService<WindowsWindowTrackingService>();

builder.WebHost.UseUrls("https://localhost:7131");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseWebSockets();
app.UseCors("BrowserPolicy");
app.UseHttpsRedirection();

app.MapGet("/", () => "SignalR server running");
app.MapHub<TabFocus>("/tabfocused");
app.MapHub<AppFocusHub>("/appfocused");

app.Run();
