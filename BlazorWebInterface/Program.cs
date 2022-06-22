using BlazorWebInterface.Pages;
using CringeForestLibrary;
using Microsoft.AspNetCore.SignalR;
using WebInterface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR().AddMessagePackProtocol();
builder.Services.AddServerSideBlazor();
var mapViewer = new MapViewer();
var statisticsViewer = new StatisticsViewer();
builder.Services.AddSingleton(new CringeForest(mapViewer, statisticsViewer));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<MapHub>("/simulation");
app.MapHub<StatisticsHub>("/statistics");
app.MapFallbackToPage("/_Host");

app.Use(async (context, next) =>
{
    mapViewer.HubContext = context.RequestServices.GetRequiredService<IHubContext<MapHub>>();
    statisticsViewer.HubContext = context.RequestServices.GetRequiredService<IHubContext<StatisticsHub>>();

    await next.Invoke();
});

app.Run();