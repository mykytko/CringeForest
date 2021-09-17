using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebInterface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        // GET
        //public IActionResult Index()
        //{
        //    return View();
        //}
        
        // POST
        [HttpPost]
        //public ActionResult<string> Post([FromBody] string command)
        public async Task<string> Post()
        {
            StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
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
                
                if (!Program.cringeForest.LoadMap(tokens[1]))
                {
                    return "The map is invalid or does not exist";
                }
                return "The map loaded successfully";
            }
            
            switch (command)
            {
                case "InitializeSimulation":
                    Task.Run(Program.cringeForest.InitializeSimulation);
                    return "Simulation initialized";
                case "ResumeSimulation":
                    Program.cringeForest.ResumeSimulation();
                    return "Simulation resumed";
                case "StopSimulation":
                    Program.cringeForest.StopSimulation();
                    return "Simulation stopped";
                case "SaveMap":
                    Program.cringeForest.SaveMap();
                    return "Map saved";
                default:
                    return "Invalid command";
            }
        }
        
        // TODO: write a PUT request for ChangeInitialParameters()
    }
}