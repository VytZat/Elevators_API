using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Elevators_API
{
    public class Program
    {
        public static bool dbInitialized = false;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
