using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class AnimalSimulation
    {
        private readonly Map _map;

        public AnimalSimulation(in Map map)
        {
            Trace.WriteLine("Initializing the animal simulation...");
            _map = map;
            Animal.SetMap(in map);
        }

        public void SimulateTick()
        {
            var animals = _map.EnumerateAnimals();
            var dict = new Dictionary<int, int>();
            foreach (var animal in animals)
            {
                animal.Value.Act(in _map, in animals);
                if (animal.Value.Saturation <= 0)
                {
                    _map.DeleteAnimal(animal.Value.Position);
                }
                if (!dict.ContainsKey(animal.Value.Type))
                {
                    dict.Add(animal.Value.Type, 1);
                }
                else
                {
                    dict[animal.Value.Type] += 1;
                }
            }

            Trace.WriteLine("The populations are:");
            foreach (var type in dict.Keys)
            {
                Trace.WriteLine(dict[type] + " of " + Metadata.AnimalSpecifications[type].Name);
            }
        }
    }
}