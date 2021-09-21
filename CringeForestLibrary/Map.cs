using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CringeForestLibrary
{
    public class Map
    {
        private IMapViewer _mapViewer;
        public int Width { get; }
        public int Height { get; }

        private class Pixel
        {
            public int BiomeId { get; set; }
        }
        
        private Pixel[,] _matrix;
        private Dictionary<(int, int), Animal> _animals;
        private Dictionary<(int, int), Food> _food;

        public Map(IMapViewer mapViewer, int width = 256, int height = 256)
        {
            Trace.WriteLine("Preparing the map...");
            Width = width;
            Height = height;
            _matrix = new Pixel[Width, Height];
            _animals = new Dictionary<(int, int), Animal>();
            _food = new Dictionary<(int, int), Food>();
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

        public void AddFood((int, int) coords, Food food)
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