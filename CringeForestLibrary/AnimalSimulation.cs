using System;
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
        }

        public void SimulateTick()
        {
            var animals = _map.EnumerateAnimals();
            foreach (var animal in animals.Values)
            {
                animal.Act(in _map, in animals);
            }
        }
    }
}