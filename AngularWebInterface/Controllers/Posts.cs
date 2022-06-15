using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AngularWebInterface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        // POST
        [HttpPost]
        public async Task<string> Post()
        {
            var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string command = await reader.ReadToEndAsync();
            Console.WriteLine("COMMAND: " + command);
            /////////////////////////
            string[] tokens = command.Split(' ');
            if (tokens[0] == "LoadMap")
            {
                if (tokens.Length == 1)
                {
                    return "No map file specified";
                }

                if (tokens.Length > 2)
                {
                    return "Too many arguments";
                }
                
                if (!Program.CringeForest.LoadMap(tokens[1]))
                {
                    return "The map is invalid or does not exist";
                }
                
                return "The map loaded successfully";
            }
            
            switch (command)
            {
                case "InitializeSimulation":
                    Task.Run(async () => Program.CringeForest.InitializeSimulation(
                        await HttpContext.WebSockets.AcceptWebSocketAsync()));
                    return "Simulation initialized";
                case "ResumeSimulation":
                    Program.CringeForest.ResumeSimulation();
                    return "Simulation resumed";
                case "StopSimulation":
                    Program.CringeForest.StopSimulation();
                    return "Simulation stopped";
                case "SaveMap":
                    Program.CringeForest.SaveMap();
                    return "Map saved";
                default:
                    return "Invalid command";
            }
        }
        
        // TODO: write a PUT request for ChangeInitialParameters()
    }
}