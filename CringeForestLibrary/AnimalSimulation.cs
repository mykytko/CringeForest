using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class AnimalSimulation
    {
        private readonly Map _map;
        private readonly Dictionary<(int, int), Animal> _animals = new ();

        private static int DetermineType(Random rand, int biomeId)
        {
            var roll = rand.Next(100);
            var animalType = 0;
            var sum = 0;
            while (true)
            {
                sum += Metadata.BiomeSpecifications[biomeId].AnimalShares[animalType];
                if (roll <= sum)
                {
                    break;
                }
                animalType++;
            }

            return animalType;
        }
        
        public AnimalSimulation(in Map map)
        {
            Trace.WriteLine("Constructing the simulation...");
            _map = map;

            var rand = new Random();
            var baselineProbability = 100.0 / (_map.Height * _map.Width);
            for (var i = 0; i < _map.Height; i++)
            {
                for (var j = 0; j < _map.Width; j++)
                {
                    Trace.WriteLine("Pixel (" + i + ", " + j + "), checking probabilities...");
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    var biomeId = _map.GetPixelBiome((i, j));
                    var animalType = DetermineType(rand, biomeId);
                    var animal = new Animal(animalType, 
                        rand.Next(2) != 0 ? AnimalSex.Female : AnimalSex.Male, (i, j));
                    _animals.Add((i, j), animal);
                    _map.AddAnimal((i, j), animal);
                }
            }

            Trace.WriteLine("Simulation constructed.");
        }

        public void SimulateTick()
        {
            foreach (var animal in _animals.Values)
            {
                animal.Act(in _map, in _animals);
            }
        }
    }
}