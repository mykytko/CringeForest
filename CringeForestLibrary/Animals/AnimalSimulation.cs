using System.Collections.Generic;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public class AnimalSimulation
    {
        private readonly Map _map;
        private readonly IStatisticsViewer _statisticsViewer;

        public AnimalSimulation(in Map map, IStatisticsViewer statisticsViewer)
        {
            Trace.WriteLine("Initializing the animal simulation...");
            _map = map;
            Animal.SetMap(in map);
            _statisticsViewer = statisticsViewer;
        }

        public void SimulateTick()
        {
            var animals = _map.AnimalsById;
            var dict = new Dictionary<string, int>();
            foreach (var animal in animals)
            {
                animal.Value.Act(in _map);
                var name = Metadata.AnimalSpecifications[animal.Value.Type].Name;
                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, 1);
                }
                else
                {
                    dict[name] += 1;
                }
            }
            _map.ClearAnimals();
            _map.GrowFood();
            
            _statisticsViewer.UpdateStatistics(dict);
            
            Trace.WriteLine("The populations are:");
            foreach (var name in dict.Keys)
            {
                Trace.WriteLine(dict[name] + " of " + name);
            }
        }
    }
}