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
    public static class Program
    {
        public static CringeForest CringeForest { get; private set; }

        public static void Main(string[] args)
        {
            var mapViewer = new MapViewer();
            CringeForest = new CringeForest(mapViewer);
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}