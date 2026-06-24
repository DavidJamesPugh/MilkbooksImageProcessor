using MilkbooksImageProcessor.Services.Interfaces;
using MilkbooksImageProcessor.Services;
using MilkbooksImageProcessor.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Render (and most cloud platforms) assign a port via the PORT env var.
// ASPNETCORE_URLS overrides the default so the app listens on the right port.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddHttpClient<IDownloadService, DownloadService>();
builder.Services.AddScoped<IResizeService, ResizeService>();


//Add global rate limit. We would need to extend this to be for each client
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("images", policy =>
    {
        policy.PermitLimit = 50;
        policy.Window = TimeSpan.FromHours(1);
        policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policy.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddSingleton<RateLimitCounterService>();
builder.Services.AddHostedService<ImageCleanupService>();

builder.Services.AddControllers();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.UseRateLimiter();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("/app/index.html");

app.Run();
