using System.Diagnostics;

namespace CringeForestLibrary
{
    public class AnimalSimulation
    {
        private Map _map;
        
        public AnimalSimulation(in Map map)
        {
            Trace.WriteLine("Constructing the simulation...");
            _map = map;
            Trace.WriteLine("Simulation constructed.");
        }
    }
}