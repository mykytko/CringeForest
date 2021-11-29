using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        private readonly Pixel[,] _matrix;
        private readonly Dictionary<(int, int), FoodSupplier> _food;
        private readonly Dictionary<(int, int), Animal> _animals;

        public Map(IMapViewer mapViewer, int height, int width, Pixel[,] matrix, Dictionary<(int, int), FoodSupplier> food, Dictionary<(int, int), Animal> animals)
        {
            Trace.WriteLine("Preparing the map...");
            Height = height;
            Width = width;
            _matrix = matrix;
            _food = food;
            _animals = animals;
            _mapViewer = mapViewer;
            Trace.WriteLine("The map is initialized");
        }
        public int GetPixelBiome((int, int) coords)
        {
            return _matrix[coords.Item1, coords.Item2].BiomeId;
        }

        public void AddAnimal((int, int) coords, Animal animal)
        {
            _mapViewer.AddAnimalView(coords, animal);
        }

        public void DeleteAnimal((int, int) coords)
        {
            _mapViewer.DeleteAnimalView(coords);
        }

        public void MoveAnimal((int, int) coords1, (int, int) coords2)
        {
            _mapViewer.MoveAnimalView(coords1, coords2);
        }

        public void AddFood((int, int) coords, FoodSupplier food)
        {
            _food.Add(coords, food);
            _mapViewer.AddFoodView(coords, food);
        }

        public void UpdateFood((int, int) coords)
        {
            _mapViewer.SetFoodView(coords, _food[coords].Saturation);
        }

        public FoodSupplier GetFood((int, int) coords)
        {
            return _food[coords];
        }
    }
}