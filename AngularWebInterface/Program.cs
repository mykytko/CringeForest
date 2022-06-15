using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using CringeForestLibrary;

namespace AngularWebInterface
{
    public static class Program
    {
        public static CringeForest CringeForest { get; private set; }

        public static void Main(string[] args)
        {
            var mapViewer = new MapViewer(new WebSocketManager());
            CringeForest = new CringeForest(mapViewer);
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}