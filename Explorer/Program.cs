using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Explorer
{
    /// <summary>
    ///     Class containing the entry point to the program.
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     Entry point to the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                  .CreateDefaultBuilder(args)
                  .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
