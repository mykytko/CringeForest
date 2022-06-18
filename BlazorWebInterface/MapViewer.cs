using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWebInterface.Pages;
using CringeForestLibrary;
using Microsoft.AspNetCore.SignalR;
using WebInterface.Serializable;

namespace WebInterface
{
    public class MapViewer : IMapViewer
    {
        public IHubContext<MapHub> HubContext;
        
        public async void SetInitialView(Map map)
        {
            await HubContext.Clients.All.SendAsync("ReceiveMap", 
                JsonSerializer.Serialize(new SerializableMap(map)));
        }
        
        public void AddAnimalView((int, int) coords, Animal animal)
        {
            
        }

        public void DeleteAnimalView((int, int) coords)
        {
            
        }

        public void MoveAnimalView((int, int) coords1, (int, int) coords2)
        {
            
        }

        public void AddFoodView((int, int) coords, FoodSupplier food)
        {
            
        }

        public async void SetFoodView((int, int) coords, double ratio)
        {
            await HubContext.Clients.All.SendAsync("ReceiveFoodRatio",
                JsonSerializer.Serialize(new CoordsAndRatio(coords, ratio)));
        }
    }
}