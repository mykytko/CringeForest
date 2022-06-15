using CringeForestLibrary;
using Microsoft.AspNetCore.SignalR;

namespace WebInterface;

public class MapHub : Hub
{
    public Task SendBackgroundData(Map map)
    {
        return Clients.All.SendAsync("SendBackgroundData", map);
    }
}