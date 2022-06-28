using System;
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
            private int _biomeId;
            public int BiomeId
            {
                get => _biomeId;
                private init
                {
                    if (value == -1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                
                    _biomeId = value;
                }
            }
            
            public Pixel(int biomeId)
            {
                BiomeId = biomeId;
            }
        }

        public Pixel[,] Matrix { get; }
        public Dictionary<(int, int), FoodSupplier> Food { get; }
        public ConcurrentDictionary<int, Animal> AnimalsById { get; }
        public Dictionary<(int, int), int> AnimalIdByPos { get; }
        private readonly List<int> _animalsToDelete = new();

        /*
        * Makes copy of given map and stores it into inner fields
        * (sizes, tables of pixels, foods(decoded), animals)
        */
        public Map(SerializableMap map, IMapViewer mapViewer)
        {
            Height = map.Height;
            Width = map.Width;
            Matrix = new Pixel[Height, Width];
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    Matrix[i, j] = new Pixel(map.Matrix[i][j]);
                }
            }

            Food = new Dictionary<(int, int), FoodSupplier>();
            foreach (var (key, value) in map.FoodSuppliers)
            {
                var x = key / Height;
                var y = key % Height;
                Food.Add((x, y), value);
            }

            AnimalIdByPos = new Dictionary<(int, int), int>();
            AnimalsById = new ConcurrentDictionary<int, Animal>();
            foreach (var (key, value) in map.Animals)
            {
                AnimalsById.TryAdd(key, value);
                AnimalIdByPos.Add(value.Position(), key);
            }

            _mapViewer = mapViewer;
        }
        
        /*
         * Puts received sizes, tables of pixels, foods, animals into inner fields
         */
        public Map(IMapViewer mapViewer, int height, int width, Pixel[,] matrix, 
            Dictionary<(int, int), FoodSupplier> food, ConcurrentDictionary<int, Animal> animals)
        {
            Trace.WriteLine("Preparing the map...");
            Height = height;
            Width = width;
            Matrix = matrix;
            Food = food;
            AnimalsById = animals;
            AnimalIdByPos = new Dictionary<(int, int), int>();
            foreach (var animal in animals)
            {
                AnimalIdByPos.Add(animal.Value.Position(), animal.Key);
            }
            _mapViewer = mapViewer;
            Trace.WriteLine("The map is initialized");
        }

        public void AddAnimal((int, int) coords, Animal animal)
        {
            (animal.X, animal.Y) = coords;
            AnimalsById.TryAdd(animal.Id, animal);
            AnimalIdByPos.Add(coords, animal.Id);
            _mapViewer.AddAnimalView(animal);
        }

        public void DeleteAnimalByPos((int, int) coords)
        {
            AnimalIdByPos.Remove(coords, out var id);
            _animalsToDelete.Add(id);
            _mapViewer.DeleteAnimalView(id);
        }

        public void ClearAnimals()
        {
            foreach (var id in _animalsToDelete)
            {
                AnimalsById.TryRemove(id, out _);
            }
            _animalsToDelete.Clear();
        }

        /*
         * Attempts to increase the saturation of all food by the constant amount of growth,
         * but not more than the maximum for that type of food
         * BTW: the growth rate of food should be adjusted manually
         * so that the simulation runs as long and dynamically as possible
         * growthRate should be moved to specification.json
         * and have different value for different food types
         */
        public void GrowFood()
        {
            const int growthRate = 2;
            foreach (var (coords, food) in Food)
            {
                var maxSaturation = Metadata.FoodSpecifications[food.FoodType].Saturation;
                if (food.Saturation < maxSaturation - growthRate)
                {
                    food.Saturation += growthRate;
                    UpdateFood(coords);
                }
                else if (food.Saturation < maxSaturation)
                {
                    food.Saturation = maxSaturation;
                    UpdateFood(coords);
                }
            }
        }

        public void MoveAnimal((int, int) coords1, (int, int) coords2)
        {
            AnimalIdByPos.Remove(coords1, out var id);
            AnimalIdByPos.Add(coords2, id);
            (AnimalsById[id].X, AnimalsById[id].Y) = coords2;
            
            _mapViewer.MoveAnimalView(id, coords2);
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