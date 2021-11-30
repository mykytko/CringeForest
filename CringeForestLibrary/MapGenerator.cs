using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public static class MapGenerator
    {
        public static Map GenerateMap(IMapViewer mapViewer, int height = 256, int width = 256)
        {
            Trace.WriteLine("Generating the map...");
            var terrain = GenerateTerrain(height, width);
            var food = GenerateFoodSuppliers(terrain, height, width);
            var animals = GenerateAnimals(terrain, height, width);
            var map = new Map(mapViewer, height, width, terrain, food, animals);
            Trace.WriteLine("The map has been generated.");
            return map;
        }

        private static Map.Pixel[,] GenerateTerrain(int height, int width)
        {
            var biggestDimension = (height > width ? height : width);
            var perlinNoise = new PerlinNoise(biggestDimension);
            var terrain = new Map.Pixel[height, width];
            for(var i = 0; i < height; i++)
            {
                for(var j = 0; j < width; j++)
                {
                    terrain[i, j] = SelectBiome(perlinNoise.GenerateNoise(i, j));
                }
            }
            return terrain;
        }
        private static Map.Pixel SelectBiome(double value)
        {
            var biomeId = -1;
            for(var i = 0; i < Metadata.BiomeSpecifications.Count; i++)
            {
                if (value <= Metadata.BiomeSpecifications[i].LowerBound ||
                    value >= Metadata.BiomeSpecifications[i].UpperBound)
                {
                    continue;
                }
                biomeId = i;
                break;
            }
            var pixel = new Map.Pixel(biomeId);
            return pixel;
        }

        private static Dictionary<(int, int), FoodSupplier> GenerateFoodSuppliers(in Map.Pixel[,] terrain, 
            int height, int width)
        {
            var food = new Dictionary<(int, int), FoodSupplier>();
            var rand = new Random();
            const double averageFoodAmount = 100.0;
            var baselineProbability = averageFoodAmount / (height * width);
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    var biomeId = terrain[i, j].BiomeId;
                    var foodType = DetermineFoodType(rand, biomeId);
                    if (foodType == -1)
                    {
                        continue;
                    }
                    var foodSupplier = new FoodSupplier(foodType);
                    food.Add((i, j), foodSupplier);
                    Trace.WriteLine(Metadata.FoodSpecifications[foodType].Name 
                                    + " at position (" + i + ", " + j + ") was created");
                }
            }
            return food;
        }
        private static int DetermineFoodType(Random rand, int biomeId)
        {
            var roll = rand.Next(100);
            var sum = 0;
            var result = -1;
            foreach (var (foodId, foodShare) in Metadata.BiomeSpecifications[biomeId].FoodShares)
            {
                sum += foodShare;
                if (roll > sum) continue;
                result = foodId;
                break;
            }

            return result;
        }

        private static Dictionary<(int, int), Animal> GenerateAnimals(in Map.Pixel[,] terrain, int height, int width)
        {
            var animals = new Dictionary<(int, int), Animal>();
            var rand = new Random();
            const double averageAnimalAmount = 100.0;
            var baselineProbability = averageAnimalAmount / (height * width);
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    var biomeId = terrain[i, j].BiomeId;
                    var animalType = DetermineAnimalType(rand, biomeId);
                    if (animalType == -1)
                    {
                        continue;
                    }
                    var animal = new Animal(animalType,
                        rand.Next(2) != 0 ? AnimalSex.Female : AnimalSex.Male, (i, j));
                    animals.Add((i, j), animal);
                }
            }
            return animals;
        }
        
        private static int DetermineAnimalType(Random rand, int biomeId)
        {
            var roll = rand.Next(100);
            var sum = 0;
            var result = -1;
            foreach (var (animalId, animalShare) in Metadata.BiomeSpecifications[biomeId].AnimalShares)
            {
                sum += animalShare;
                if (roll > sum) continue;
                result = animalId;
                break;
            }

            return result;
        }
        
        private class PerlinNoise
        {
            //TODO1: work with seed: make seed-influenced: parameters below, biome boundaries,
            //average*Amount in MapGenerator.Generate*(also influenced by map size), seed for var rand in
            //MapGenerator.Generate*
            //TODO2: work with biome boundaries in json and MapGenerator.SelectBiome: improve,
            //make seed-influenced, generate different regions, were boundaries will be quite different
            //TODO3: extend json variety, change spawn-rate type from int to double
            //TODO: comments(also with basic values for fields), renames(2 moments), formatting,
            //access modifiers(readonly included), excaptions catch, static classes and constructors and generics,
            //pair coordinates into one type (int, int)
            
            private readonly int _size;
            private readonly int[] _permutation;
            private readonly int[] _p;
            private readonly double _aFade;
            private readonly double _bFade;
            private readonly double _cFade;
            private readonly int _octaves;
            private readonly double _startScale;
            private readonly double _startInfluence;
            private readonly double _scaleMultiplier;
            private readonly double _influenceMultiplier;

            public PerlinNoise(int size = 256)
            {
                _size = size;

                _permutation = new int[_size];
                _p = new int[_size * 2];
                for (var i = 0; i < _size; i++)
                {
                    _permutation[i] = i;
                }
                MakePermutation();
                for (int i = 0; i < _size * 2; i++)
                {
                    _p[i] = _permutation[i % _size];
                }

                _aFade = 6;
                _bFade = -15;
                _cFade = 10;

                _octaves = 1;
                _startScale = 0.05;
                _startInfluence = 1;
                _scaleMultiplier = 2;
                _influenceMultiplier = 0.2;
            }

            public double GenerateNoise(double x, double y)
            {
                double sum = 0;
                double maxSum = 0;
                var scale = _startScale;
                var influence = _startInfluence;
                for (var i = 0; i < _octaves; i++)
                {
                    sum += MakeOneNoise(x * scale, y * scale) * influence;
                    maxSum += influence;
                    scale *= _scaleMultiplier;
                    influence *= _influenceMultiplier;
                }

                return sum / maxSum;
            }

            private double MakeOneNoise(double x, double y)
            {
                var xi = (int)x % _size;
                var yi = (int)y % _size;
                var xf = x - (int)x;
                var yf = y - (int)y;

                var u = Fade(xf);
                var v = Fade(yf);

                var g1 = _p[_p[xi] + yi];
                var g2 = _p[_p[Inc(xi)] + yi];
                var g3 = _p[_p[xi] + Inc(yi)];
                var g4 = _p[_p[Inc(xi)] + Inc(yi)];

                var d1 = Grad(g1, xf, yf);
                var d2 = Grad(g2, xf - 1, yf);
                var d3 = Grad(g3, xf, yf - 1);
                var d4 = Grad(g4, xf - 1, yf - 1);

                var x1Inter = Interpolate(u, d1, d2);
                var x2Inter = Interpolate(u, d3, d4);
                var yInter = Interpolate(v, x1Inter, x2Inter);

                return (yInter + 1) / 2;
            }

            private void MakePermutation()
            {
                var rand = new Random();
                for (var i = _size - 1; i >= 0; i--)
                {
                    var j = rand.Next(0, i + 1);
                    (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
                }
            }
            private double Fade(double x)
            {
                return x * x * x * (x * (x * _aFade + _bFade) + _cFade);
            }
            private static int Inc(int n)
            {
                n++;
                return n;
            }
            private static double Grad(int hash, double x, double y)
            {
                return (hash & 3) switch
                {
                    0 => x + y,
                    1 => -x + y,
                    2 => x - y,
                    3 => -x - y,
                    _ => 0
                };
            }
            private static double Interpolate(double amount, double left, double right)
            {
                return (1 - amount) * left + amount * right;
            }
        }
    }
}