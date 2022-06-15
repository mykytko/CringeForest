using CringeForestLibrary;

namespace WebInterface
{
    public class MapViewer : IMapViewer
    {
        public async void SetBackgroundView(Map map)
        {
            
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