using System.Text.Json;
using BlazorWebInterface.Pages;
using CringeForestLibrary;
using Microsoft.AspNetCore.SignalR;

namespace WebInterface;

public class StatisticsViewer : IStatisticsViewer
{
    public IHubContext<StatisticsHub> HubContext;
    
    public async void UpdateStatistics(Dictionary<string, int> dict)
    {
        await HubContext.Clients.All.SendAsync("ReceiveStatistics",
            JsonSerializer.Serialize(dict));
    }
}