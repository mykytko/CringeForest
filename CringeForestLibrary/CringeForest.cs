// Facade class

using System;
using System.IO;
using System.Diagnostics;

namespace CringeForestLibrary
{
    public interface IMapViewer
    {
        public void AddAnimalView((int, int) coords, Animal animal);
        public void DeleteAnimalView((int, int) coords);
        public void MoveAnimalView((int, int) coords1, (int, int) coords2);
        public void AddFoodView((int, int) coords, FoodSupplier food);
        public void SetFoodView((int, int) coords, int saturation);
        void SetBackgroundView(Map map);
    }
    
    public class CringeForest
    {
        private AnimalSimulation _animalSimulation;
        private int _age;
        private bool _isStopped;
        private bool _isResumed;
        private bool _exitProgram;
        private float _simulationSpeed = 1.0f;
        private const string DefaultSavedMapName = "savedMap";
        private const string MapExtension = ".cfm";
        private const string DefaultJsonFileName = "ObjectTypesSpecification.json";
        private static IMapViewer _mapViewer;

        public void Initialize(IMapViewer mapViewer)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText("CringeForest.log")));
            Trace.AutoFlush = true;
            if (!Metadata.InitializeMetadata(DefaultJsonFileName))
            {
                Trace.WriteLine("Critical Error! Couldn't load metadata. Terminating...");
                Environment.Exit(1);
            }

            _mapViewer = mapViewer;
        }
        
        public string SaveMap()
        {
            var mapName = DefaultSavedMapName + MapExtension;
            if (File.Exists(mapName))
            {
                mapName = DefaultSavedMapName + "1" + MapExtension;
            }

            for (int i = 1;; i++)
            {
                if (File.Exists(mapName))
                {
                    mapName = DefaultSavedMapName + i + MapExtension;
                }
                else
                {
                    break;
                }
            }

            var mapFileStream = new FileStream(mapName, FileMode.CreateNew);
            // TODO: write the map from memory to mapFileStream
            // terrain is setted by size and seed
            // food-array contains foodID+coordinates+amount of food
            // animal-array contains animalID+coordinates
            return mapName;
        }
        public bool LoadMap(string path)
        {
            // should return false if map is not found
            if (!File.Exists(path))
            {
                return false;
            }
            var mapFileStream = new FileStream(path, FileMode.Open);
            // TODO: read the map and load into memory
            // you should check for a loaded map in InitializeSimulation()
            // if there is no map loaded, generate a new one
            return true;
        }

        public bool LoadParameters(string path)
        {
            // should return false if file is not found or the configuration is invalid
            if (!File.Exists(path))
            {
                return false;
            }
            var parametersFileStream = new FileStream(path, FileMode.Open);
            // TODO: load parameters into memory
            // you should check for loaded parameters in InitializeSimulation()
            // if there are no parameters loaded, use default
            
            return true;
        }

        public void InitializeSimulation()
        {
            // TODO: check for a loaded map or parameters,
            // if none are found - use default params and/or generate a new map
            // after everything is loaded, we can start the simulation
            try
            {
                var map = MapGenerator.GenerateMap(_mapViewer);
                _animalSimulation = new AnimalSimulation(map);
                _mapViewer.SetBackgroundView(map);

                MainLoop();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message + " " + e.StackTrace);
            }
            
        }

        public void StopSimulation()
        {
            _isStopped = true;
        }

        public void ExitSimulation()
        {
            _exitProgram = true;
        }

        public void ResumeSimulation()
        {
            _isResumed = true;
        }

        public void SetSimulationSpeed(float newSpeed)
        {
            _simulationSpeed = newSpeed;
        }

        private void MainLoop()
        {
            Trace.WriteLine("Simulation started.");
            while (true)
            {
                if (_isStopped)
                {
                    while (!_isResumed && !_exitProgram)
                    {
                    }

                    if (_isResumed)
                    {
                        _isResumed = false;
                        _isStopped = false;
                    }

                    if (_exitProgram)
                    {
                        break;
                    }
                }

                Trace.WriteLine("Tick " + _age + " start");
                _animalSimulation.SimulateTick();
                _age++;
                Trace.WriteLine("Tick " + _age + " end");
            }
        }
    }
}