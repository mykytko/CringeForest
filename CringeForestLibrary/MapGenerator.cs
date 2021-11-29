using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class MapGenerator
    {
        public Map GenerateMap(IMapViewer mapViewer, int height = 256, int width = 256)
        {
            Trace.WriteLine("Constructing the simulation...");
            Map.Pixel[,] terrain = GenerateTerrain(height, width);
            Dictionary<(int, int), FoodSupplier> food = GenerateFood(terrain, height, width);
            Dictionary<(int, int), Animal> animals = GenerateAnimal(terrain, height, width);
            Map map = new Map(mapViewer, height, width, terrain, food, animals);
            Trace.WriteLine("Simulation constructed.");
            return map;
        }

        private Map.Pixel[,] GenerateTerrain(int height, int width)
        {
            int biggestDimension = (height > width ? height : width);
            PerlinNoise perlinNoise = new PerlinNoise(biggestDimension);
            Map.Pixel[,] terrain = new Map.Pixel[height, width];
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    terrain[i, j] = SelectBiome(perlinNoise.GenerateNoise(i, j));
                }
            }
            return terrain;
        }
        private Map.Pixel SelectBiome(double value)
        {
            int BiomeID = -1;
            for(int i = 0; i < Metadata.BiomeSpecifications.Count; i++)
            {
                if(value > Metadata.BiomeSpecifications[i].LowerBound && value < Metadata.BiomeSpecifications[i].UpperBound)
                {
                    BiomeID = i;
                    break;
                }
            }
            Map.Pixel pixel = new Map.Pixel(BiomeID);
            return pixel;
        }

        private Dictionary<(int, int), FoodSupplier> GenerateFood(in Map.Pixel[,] terrain, int height, int width)
        {
            Dictionary<(int, int), FoodSupplier> food = new Dictionary<(int, int), FoodSupplier>();
            var rand = new Random();
            var averageFoodAmount = 100.0;
            var baselineProbability = averageFoodAmount / (height * width);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Trace.WriteLine("Pixel (" + i + ", " + j + "), checking probabilities...");
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    int biomeID = terrain[i, j].BiomeId;
                    int foodType = DetermineFoodType(rand, biomeID);
                    var foodSupplier = new FoodSupplier(foodType);
                    food.Add((i, j), foodSupplier);
                }
            }
            return food;
        }
        private int DetermineFoodType(Random rand, int biomeID)
        {
            var roll = rand.Next(100);
            var foodType = 0;
            var sum = 0;
            while (true)
            {
                sum += Metadata.BiomeSpecifications[biomeID].FoodShares[foodType];
                if (roll <= sum)
                {
                    break;
                }
                foodType++;
            }

            return foodType;
        }

        private Dictionary<(int, int), Animal> GenerateAnimal(in Map.Pixel[,] terrain, int height, int width)
        {
            Dictionary<(int, int), Animal> animals = new Dictionary<(int, int), Animal>();
            var rand = new Random();
            var averageAnimalAmount = 100.0;
            var baselineProbability = averageAnimalAmount / (height * width);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Trace.WriteLine("Pixel (" + i + ", " + j + "), checking probabilities...");
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    int biomeID = terrain[i, j].BiomeId;
                    int animalType = DetermineAnimalType(rand, biomeID);
                    var animal = new Animal(animalType,
                        rand.Next(2) != 0 ? AnimalSex.Female : AnimalSex.Male, (i, j));
                    animals.Add((i, j), animal);
                }
            }
            return animals;
        }
        private int DetermineAnimalType(Random rand, int biomeID)
        {
            var roll = rand.Next(100);
            var animalType = 0;
            var sum = 0;
            while (true)
            {
                sum += Metadata.BiomeSpecifications[biomeID].AnimalShares[animalType];
                if (roll <= sum)
                {
                    break;
                }
                animalType++;
            }

            return animalType;
        }
        private class PerlinNoise
        {
            //TODO1: work with seed: make seed-influenced: parametrs below, biome boundaries, avarage*Amount in MapGenerator.Generate*(also influenced by map size), seed for var rand in MapGenerator.Generate*
            //TODO2: work with biome boundaries in json and MapGenerator.SelectBiome: improve, make seed-influenced, generate different regions, were boundaries will be quite different
            //TODO3: extend json variety, change spawn-rate type from int to double
            //TODO: comments(also with basic values for fields), renames(2 moments), formating, acces modificators(readonly included), excaptions catch, static classes and constructors and generics, pair coordinates into one type (int, int)
            private int _size;
            private int[] _permutation;
            private int[] _p;
            private double _a_fade, _b_fade, _c_fade;
            private int _octaves;
            private double _startScale;
            private double _startInfluence;
            private double _scaleMultiplier;
            private double _influenceMultiplier;

            public PerlinNoise(int size = 256)
            {
                _size = size;

                _permutation = new int[_size];
                _p = new int[_size * 2];
                for (int i = 0; i < _size; i++)
                {
                    _permutation[i] = i;
                }
                MakePermutation();
                for (int i = 0; i < _size * 2; i++)
                {
                    _p[i] = _permutation[i % _size];
                }

                _a_fade = 6;
                _b_fade = -15;
                _c_fade = 10;

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
                double scale = _startScale;
                double influence = _startInfluence;
                for (int i = 0; i < _octaves; i++)
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
                int xi = (int)x % _size;
                int yi = (int)y % _size;
                double xf = x - (int)x;
                double yf = y - (int)y;

                double u = Fade(xf);
                double v = Fade(yf);

                int g1 = _p[_p[xi] + yi];
                int g2 = _p[_p[Inc(xi)] + yi];
                int g3 = _p[_p[xi] + Inc(yi)];
                int g4 = _p[_p[Inc(xi)] + Inc(yi)];

                double d1 = Grad(g1, xf, yf);
                double d2 = Grad(g2, xf - 1, yf);
                double d3 = Grad(g3, xf, yf - 1);
                double d4 = Grad(g4, xf - 1, yf - 1);

                double x1Inter = Interpolate(u, d1, d2);
                double x2Inter = Interpolate(u, d3, d4);
                double yInter = Interpolate(v, x1Inter, x2Inter);

                return (yInter + 1) / 2;
            }

            private void MakePermutation()
            {
                var rand = new Random();
                for (int i = _size - 1; i >= 0; i--)
                {
                    int j = rand.Next(0, i + 1);
                    int temp = _permutation[i];
                    _permutation[i] = _permutation[j];
                    _permutation[j] = temp;
                }
            }
            private double Fade(double x)
            {
                return x * x * x * (x * (x * _a_fade + _b_fade) + _c_fade);
            }
            private int Inc(int n)
            {
                n++;
                return n;
            }
            private double Grad(int hash, double x, double y)
            {
                switch (hash & 3)
                {
                    case 0: return x + y;
                    case 1: return -x + y;
                    case 2: return x - y;
                    case 3: return -x - y;
                    default: return 0;
                }
            }
            private double Interpolate(double amount, double left, double right)
            {
                return ((1 - amount) * left + amount * right);
            }
        }
    }
}