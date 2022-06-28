// Facade class

using System;
using System.IO;
using System.Diagnostics;
using System.Text.Json;

namespace CringeForestLibrary
{
    public class CringeForest
    {
        private AnimalSimulation _animalSimulation;
        private int _age;
        private bool _isStopped;
        private bool _isResumed;
        private bool _exitProgram;
        private const string DefaultJsonFileName = "ObjectTypesSpecification.json";
        private static IMapViewer _mapViewer;
        private static IStatisticsViewer _statisticsViewer;
        private static Map _map;

        /*
         * Creates log file
         * Initializes metadata using default json
         * Puts received viewers into inner fields 
         */
        public CringeForest(IMapViewer mapViewer, IStatisticsViewer statisticsViewer)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText("CringeForest.log")));
            Trace.AutoFlush = true;
            if (!Metadata.InitializeMetadata(DefaultJsonFileName))
            {
                Trace.WriteLine("Critical Error! Couldn't load metadata. Terminating...");
                Environment.Exit(1);
            }

            _mapViewer = mapViewer;
            _statisticsViewer = statisticsViewer;
        }
        
        public static string SaveMap()
        {
            return JsonSerializer.Serialize(new SerializableMap(_map));
        }
        
        public static void LoadMap(string map)
        {
            _map = new Map(JsonSerializer.Deserialize<SerializableMap>(map), _mapViewer);
        }
        
        /*
         * Creates components of simulation and puts them into inner fields
         * Starts the map display
         * Runs main loop of simulation 
         */
        public void InitializeSimulation()
        {
            try
            {
                _map ??= MapGenerator.GenerateMap(_mapViewer);
                _animalSimulation = new AnimalSimulation(_map, _statisticsViewer);
                _mapViewer.SetInitialView(_map);

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
            _map = null;
            _exitProgram = true;
        }

        public void ResumeSimulation()
        {
            _isResumed = true;
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
                        Trace.WriteLine("Simulation ended.");
                        break;
                    }
                }

                Trace.WriteLine("Tick " + _age + " start");
                _animalSimulation.SimulateTick();
                Trace.WriteLine("Tick " + _age + " end");
                _age++;
            }
        }
    }
}