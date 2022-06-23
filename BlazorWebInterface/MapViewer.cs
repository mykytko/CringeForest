using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWebInterface.Pages;
using BlazorWebInterface.Serializable;
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
        
        public async void AddAnimalView(Animal animal)
        {
            await HubContext.Clients.All.SendAsync("ReceiveAdd",
                JsonSerializer.Serialize(new KeyValuePair<int, CoordsAndType>(animal.Id, 
                    new CoordsAndType(animal.X, animal.Y, animal.AnimalType))));
        }

        public async void DeleteAnimalView(int id)
        {
            await HubContext.Clients.All.SendAsync("ReceiveDelete",
                JsonSerializer.Serialize(id));
        }

        public async void MoveAnimalView(int id, (int, int) coords)
        {
            await HubContext.Clients.All.SendAsync("ReceiveMove",
                JsonSerializer.Serialize(new KeyValuePair<int, Coords>(id, new Coords(coords))));
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