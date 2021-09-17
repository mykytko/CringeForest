using System;
using System.Threading.Tasks;

using CringeForestLibrary;

namespace ConsoleInterface
{
    public class Program
    {
        static void Main()
        {
            MapViewer mapViewer = new MapViewer();
            CringeForest cringeForest = new CringeForest(mapViewer);

            bool exitInitialMenu = false;
            while (true)
            {
                // dialogue:
                Console.Write("Choose an option:\n" +
                              "0. Exit program;\n" +
                              "1. Load map;\n" +
                              "2. Change initial parameters;\n" +
                              "3. Start new simulation.\n"); 
                var input = Console.ReadLine();
                bool parsed = Int32.TryParse(input, out int choice);
                if (!parsed)
                {
                    Console.WriteLine("Invalid choice!");
                    continue;
                }

                switch (choice)
                {
                    case 0:
                        return;
                    
                    case 1:
                        while (true)
                        {
                            Console.Write("Enter the map path: ");
                            string path = Console.ReadLine();
                            if (!cringeForest.LoadMap(path))
                            {
                                Console.WriteLine("The map is not found or is invalid!");
                                Console.WriteLine("Do you want to try again? (Y/n)");
                                input = Console.ReadLine();
                                if (input == "N" || input == "n") 
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("The map has been successfully loaded!");
                                break;
                            }
                        }
                        break;
                    
                    case 2:
                        while (true)
                        {
                            Console.Write("Enter the configuration file path: ");
                            string path = Console.ReadLine();
                            if (!cringeForest.LoadParameters(path))
                            {
                                Console.WriteLine("The config file is not found or is invalid!");
                                Console.WriteLine("Do you want to try again? (Y/n)");
                                input = Console.ReadLine();
                                if (input == "N" || input == "n") 
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("The configuration has been successfully loaded!");
                                break;
                            }
                        }
                        break;
                    
                    case 3:
                        // var forestTask = cringeForest.InitializeSimulation();
                        // await later
                        Task.Run(cringeForest.InitializeSimulation);
                        exitInitialMenu = true;
                        break;
                }

                if (exitInitialMenu)
                {
                    break;
                }
            }
            
            // handle everything else during the simulation
            while (true)
            {
                // enter the main menu
                if (Console.ReadLine() == "menu")
                {
                    cringeForest.StopSimulation();
                    while (true)
                    {
                        Console.Write("Choose an option:\n" +
                                      "0. Continue the simulation;\n" +
                                      "1. Show statistics;\n" +
                                      "2. Change simulation parameters;\n" +
                                      "3. Change simulation speed;\n" +
                                      "4. Save map;\n" +
                                      "5. Exit the simulation.\n");
                        string input = Console.ReadLine();
                        int choice;
                        if (!Int32.TryParse(input, out choice))
                        {
                            Console.WriteLine("Invalid choice!");
                            continue;
                        }

                        switch (choice)
                        {
                            case 0:
                                cringeForest.ResumeSimulation();
                                break;
                            case 1:
                                // TODO: get statistics from cringeForest, which gets it from animalSimulation,
                                // and handle it here
                                break;
                            case 2:
                                while (true)
                                {
                                    Console.Write("Enter the configuration file path: ");
                                    string path = Console.ReadLine();
                                    if (!cringeForest.LoadParameters(path))
                                    {
                                        Console.WriteLine("The config file is not found or is invalid!");
                                        Console.WriteLine("Do you want to try again? (Y/n)");
                                        input = Console.ReadLine();
                                        if (input == "N" || input == "n") 
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("The configuration has been successfully loaded!");
                                        break;
                                    }
                                }
                                break;
                            case 3:
                                while (true)
                                {
                                    Console.Write("Write the desired speed: ");
                                    input = Console.ReadLine();
                                    float speed;
                                    if (!float.TryParse(input, out speed))
                                    {
                                        Console.WriteLine("Invalid number!");
                                        Console.WriteLine("So you want to try again? (Y/n)");
                                        input = Console.ReadLine();
                                        if (input == "N" || input == "n")
                                        {
                                            break;
                                        }
                                        continue;
                                    }
                                    cringeForest.SetSimulationSpeed(speed);
                                    break;
                                }
                                break;
                            case 4:
                                Console.WriteLine("The map is successfully saved to " + cringeForest.SaveMap() + "!");
                                break;
                            case 5:
                                cringeForest.ExitSimulation();
                                return;
                        }
                    }
                }
            }
        }
    }
}