using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CringeForestLibrary.Hubs;

public class MapHub : Hub
{
    public async Task SendMap(Map map)
    {
        await Clients.All.SendAsync("ReceiveMap", map);
    }
}