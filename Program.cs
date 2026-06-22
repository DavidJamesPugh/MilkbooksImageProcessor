using MilkbooksImageProcessor.Services.Interfaces;
using MilkbooksImageProcessor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IDownloadService, DownloadService>();
builder.Services.AddScoped<IResizeService, ResizeService>(); 

builder.Services.AddControllers();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("/app/index.html");

app.Run();
