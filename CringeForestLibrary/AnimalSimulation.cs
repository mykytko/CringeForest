using System;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class AnimalSimulation
    {
        private readonly Map _map;

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
            var baselineProbability = 100.0 / _map.Height * _map.Width;
            for (var i = 0; i < _map.Height; i++)
            {
                for (var j = 0; j < _map.Width; j++)
                {
                    var roll1 = rand.NextDouble();
                    if (roll1 > baselineProbability)
                    {
                        continue;
                    }
                    var biomeId = _map.GetPixelBiome((i, j));
                    var animalType = DetermineType(rand, biomeId);
                    _map.AddAnimal((i, j), 
                        new Animal(animalType, rand.Next(2) != 0 ? AnimalSex.Female : AnimalSex.Male, (i, j)));
                }
            }

            Trace.WriteLine("Simulation constructed.");
        }

        public void SimulatePeriod()
        {
            // TODO: Write a simulation of every existing animal's actions turn by turn
        }
    }
}