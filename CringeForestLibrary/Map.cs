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
        public ConcurrentDictionary<int, Animal> AnimalsById { get; }
        public ConcurrentDictionary<(int, int), int> AnimalIdByPos { get; }
        private List<int> _animalsToDelete = new();
        
        public Map(IMapViewer mapViewer, int height, int width, Pixel[,] matrix, 
            Dictionary<(int, int), FoodSupplier> food, ConcurrentDictionary<int, Animal> animals)
        {
            Trace.WriteLine("Preparing the map...");
            Height = height;
            Width = width;
            Matrix = matrix;
            Food = food;
            AnimalsById = animals;
            AnimalIdByPos = new ConcurrentDictionary<(int, int), int>();
            foreach (var animal in animals)
            {
                AnimalIdByPos.TryAdd(animal.Value.Position(), animal.Key);
                Trace.WriteLine(animal.Value.Sex + " " + Metadata.AnimalSpecifications[animal.Value.Type].Name);
            }
            _mapViewer = mapViewer;
            Trace.WriteLine("The map is initialized");
        }

        public void AddAnimal((int, int) coords, Animal animal)
        {
            (animal.X, animal.Y) = coords;
            AnimalsById.TryAdd(animal.Id, animal);
            AnimalIdByPos.TryAdd(coords, animal.Id);
            _mapViewer.AddAnimalView(coords, animal);
        }

        public void DeleteAnimalByPos((int, int) coords)
        {
            AnimalIdByPos.TryRemove(coords, out _);
            _mapViewer.DeleteAnimalView(coords);
        }

        public void ClearAnimals()
        {
            foreach (var id in _animalsToDelete)
            {
                AnimalsById.TryRemove(id, out _);
            }
            _animalsToDelete.Clear();
        }

        public void MoveAnimal((int, int) coords1, (int, int) coords2)
        {
            AnimalIdByPos.TryRemove(coords1, out var id);
            AnimalIdByPos.TryAdd(coords2, id);
            (AnimalsById[id].X, AnimalsById[id].Y) = coords2;
            
            _mapViewer.MoveAnimalView(coords1, coords2);
        }

        public void UpdateFood((int, int) coords)
        {
            _mapViewer.SetFoodView(coords, 
                (double) Food[coords].Saturation / Metadata.FoodSpecifications[Food[coords].FoodType].Saturation);
        }

        public FoodSupplier GetFood((int, int) coords)
        {
            return !Food.ContainsKey(coords) ? null : Food[coords];
        }
    }
}