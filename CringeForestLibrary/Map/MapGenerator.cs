using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public static class MapGenerator
    {
        /*
         * Generates tables of landscape, food, animals according to given sizes
         * Returns map object with generated tables and received mapViewer
         * BTW: the average amount of food and animals should be adjusted manually
         * so that the simulation runs as long and dynamically as possible
         */
        public static Map GenerateMap(IMapViewer mapViewer, int height = 256, int width = 256)
        {
            const double averageAmountOfFood = 1000.0;
            const double averageAmountOfAnimals = 1000.0;
            
            Trace.WriteLine("Generating the map...");
            var terrain = GenerateTerrain(height, width);
            var foodGenerator = new GenerateFood();
            var food = foodGenerator.Generate(terrain, height, width, averageAmountOfFood);
            var animalGenerator = new GenerateAnimal();
            var animals = animalGenerator.Generate(terrain, height, width, averageAmountOfAnimals);
            var map = new Map(mapViewer, height, width, terrain, food, animals);
            Trace.WriteLine("The map has been generated.");
            return map;
        }

        private static Map.Pixel[,] GenerateTerrain(int height, int width)
        {
            var biggestDimension = height > width ? height : width;
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
                if (value < Metadata.BiomeSpecifications[i].LowerBound ||
                    value > Metadata.BiomeSpecifications[i].UpperBound)
                {
                    continue;
                }
                biomeId = i;
                break;
            }
            var pixel = new Map.Pixel(biomeId);
            return pixel;
        }
        
        //template method
        private abstract class TemplateGenerate<T>
        {
            public T Generate(in Map.Pixel[,] terrain, int height, int width, double averageAmount)
            {
                var result = InitT();
                var rand = new Random();
                var baselineProbability = averageAmount / (height * width);
                for (var i = 0; i < height; i++)
                {
                    for (var j = 0; j < width; j++)
                    {
                        var roll = rand.NextDouble();
                        if (roll > baselineProbability)
                        {
                            continue;
                        }
                        var biomeId = terrain[i, j].BiomeId;
                        var resultType = DetermineResultType(rand, biomeId);
                        if (resultType == -1)
                        {
                            continue;
                        }
                        GenerateInstance(in result, i, j, resultType, rand);
                    }
                }
                return result;
            }
            protected abstract T InitT();
            protected abstract int DetermineResultType(Random rand, int biomeId);
            protected abstract void GenerateInstance(in T collection, int i, int j, int resultType, Random rand);
        }
        
        private class GenerateFood : TemplateGenerate<Dictionary<(int, int), FoodSupplier>>
        {
            protected override Dictionary<(int, int), FoodSupplier> InitT()
            {
                return new Dictionary<(int, int), FoodSupplier>();
            }
            protected override int DetermineResultType(Random rand, int biomeId)
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
            protected override void GenerateInstance(in Dictionary<(int, int), FoodSupplier> collection, int i, int j, int resultType, Random rand)
            {
                var foodSupplier = new FoodSupplier(resultType);
                collection.Add((i, j), foodSupplier);
            }
        }
        
        private class GenerateAnimal : TemplateGenerate<ConcurrentDictionary<int, Animal>>
        {
            protected override ConcurrentDictionary<int, Animal> InitT()
            {
                return new ConcurrentDictionary<int, Animal>();
            }
            protected override int DetermineResultType(Random rand, int biomeId)
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
            protected override void GenerateInstance(
                in ConcurrentDictionary<int, Animal> collection, int i, int j, int resultType, Random rand)
            {
                var animal = new Animal(resultType,
                        rand.Next(2) != 0 ? AnimalSex.Female : AnimalSex.Male, (i, j));
                collection.TryAdd(animal.Id, animal);
            }
        }
        
        /*
         * Class for generating vector field with a smooth gradient change
         * BTW: Many fields here should be adjusted by making them smth like seed-influenced
         */
        private class PerlinNoise
        {
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