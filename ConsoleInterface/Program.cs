using CringeForestLibrary;

namespace ConsoleInterface
{
    public class Program
    {
        static void Main(string[] args)
        {
            MapViewer mapViewer = new MapViewer();
            CringeForest cringeForest = new CringeForest(mapViewer);
        }
    }
}