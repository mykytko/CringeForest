using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using CringeForestLibrary;

namespace WebInterface
{
    public class Program
    {
        public static CringeForest cringeForest;
        public static MapViewer mapViewer;
        
        public static void Main(string[] args)
        {
            mapViewer = new MapViewer();
            cringeForest = new CringeForest(mapViewer);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}