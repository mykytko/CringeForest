using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CringeForestLibrary
{
    public class Map
    {
        private readonly IMapViewer _mapViewer;
        public int Width { get; }
        public int Height { get; }

        public class Pixel
        {
            public Pixel(int biomeId)
            {
                BiomeId = biomeId;
            }

            public int BiomeId { get; }
        }
        
        private readonly Pixel[,] _matrix;
        private readonly Dictionary<(int, int), Animal> _animals;
        private readonly Dictionary<(int, int), FoodSupplier> _food;

        public int GetPixelBiome((int, int) coords)
        {
            return _matrix[coords.Item1, coords.Item2].BiomeId;
        }

        public Map(IMapViewer mapViewer, int width = 256, int height = 256)
        {
            Trace.WriteLine("Preparing the map...");
            Width = width;
            Height = height;
            _matrix = new Pixel[Width, Height];
            _animals = new Dictionary<(int, int), Animal>();
            _food = new Dictionary<(int, int), FoodSupplier>();
            _mapViewer = mapViewer;
        }

        public void AddAnimal((int, int) coords, Animal animal)
        {
            _animals.Add(coords, animal);
            _mapViewer.AddAnimal(coords, animal);
        }

        public void DeleteAnimal((int, int) coords)
        {
            _animals.Remove(coords);
            _mapViewer.DeleteAnimal(coords);
        }

        public void MoveAnimal((int, int) coords1, (int, int) coords2)
        {
            if (!_animals.ContainsKey(coords1))
            {
                throw new ArgumentException(nameof(coords1));
            }

            if (_animals.ContainsKey(coords2))
            {
                throw new ArgumentException(nameof(coords2));
            }
            _animals.Add(coords2, _animals[coords1]);
            _animals.Remove(coords1);
            _mapViewer.MoveAnimal(coords1, coords2);
        }

        public void AddFood((int, int) coords, FoodSupplier food)
        {
            _food.Add(coords, food);
            _mapViewer.AddFood(coords, food);
        }

        public void SetFood((int, int) coords, int saturation)
        {
            _food[coords].Saturation = saturation;
            _mapViewer.SetFood(coords, saturation);
        }
    }
}