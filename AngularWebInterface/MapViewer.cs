using System.ComponentModel;
using System.Net.WebSockets;
using CringeForestLibrary;

namespace AngularWebInterface
{
    public class MapViewer : IMapViewer
    {
        private readonly WebSocketManager _webSocketManager;
        
        public MapViewer(WebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
        }

        public void Initialize(WebSocket webSocket)
        {
            _webSocketManager.Initialize(webSocket);
        }
        
        public void SetBackgroundView(int height, int width, Map.Pixel[,] matrix)
        {
            _webSocketManager.SendBackgroundData(height, width, matrix);
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

        public void SetFoodView((int, int) coords, int saturation)
        {
            
        }
    }
}