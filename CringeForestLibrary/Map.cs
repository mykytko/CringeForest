using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class Map
    {
        private readonly IMapViewer _mapViewer;
        public int Height { get; }
        public int Width { get; }
        public class Pixel
        {
            public Pixel(int biomeId)
            {
                //TODO: check if biomeId == -1
                BiomeId = biomeId;
            }

            public int BiomeId { get; }
        }

        public Pixel[,] Matrix { get; }
        public Dictionary<(int, int), FoodSupplier> Food { get; }
        public ConcurrentDictionary<(int, int), Animal> Animals { get; }

        public Map(IMapViewer mapViewer, int height, int width, Pixel[,] matrix, 
            Dictionary<(int, int), FoodSupplier> food, ConcurrentDictionary<(int, int), Animal> animals)
        {
            Trace.WriteLine("Preparing the map...");
            Height = height;
            Width = width;
            Matrix = matrix;
            Food = food;
            Animals = animals;
            foreach (var animal in animals)
            {
                Trace.WriteLine(animal.Value.Sex + " " + Metadata.AnimalSpecifications[animal.Value.Type].Name);
            }
            _mapViewer = mapViewer;
            Trace.WriteLine("The map is initialized");
        }
        public int GetPixelBiome((int, int) coords)
        {
            return Matrix[coords.Item1, coords.Item2].BiomeId;
        }

        public void AddAnimal((int, int) coords, Animal animal)
        {
            Animals.TryAdd(coords, animal);
            _mapViewer.AddAnimalView(coords, animal);
        }

        public void DeleteAnimal((int, int) coords)
        {
            Animals.TryRemove(coords, out _);
            _mapViewer.DeleteAnimalView(coords);
        }

        public void MoveAnimal((int, int) coords1, (int, int) coords2)
        {
            _mapViewer.MoveAnimalView(coords1, coords2);
        }

        public void AddFood((int, int) coords, FoodSupplier food)
        {
            Food.Add(coords, food);
            _mapViewer.AddFoodView(coords, food);
        }

        public void UpdateFood((int, int) coords)
        {
            _mapViewer.SetFoodView(coords, Food[coords].Saturation);
        }

        public FoodSupplier GetFood((int, int) coords)
        {
            return !Food.ContainsKey(coords) ? null : Food[coords];
        }

        public ConcurrentDictionary<(int, int), Animal> EnumerateAnimals()
        {
            return Animals;
        }
    }
}