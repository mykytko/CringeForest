// Facade class

using System.ComponentModel;

namespace CringeForestLibrary
{
    public interface IMapViewerInterface
    {
        public void UpdateAnimalPosition()
        {
            
        }

        public void UpdateFoodQuantity()
        {
            
        }
    }
    
    public class CringeForest
    {
        private MapHandler mapHandler;
        private AnimalSimulation animalSimulation;
        private int age = 0;
        public CringeForest(IMapViewerInterface mapViewer)
        {
            Initialize();
            while (true)
            {
                // mapHandler.GrowFood();
                // animalSimulation.moveAnimals();
                age++;
            }
        }

        private void Initialize()
        {
            mapHandler = new MapHandler();
            animalSimulation = new AnimalSimulation();
        }
    }
}