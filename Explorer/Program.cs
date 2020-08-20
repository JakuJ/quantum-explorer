using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Explorer.Tests")]

namespace Explorer
{
    /// <summary>
    /// Class containing the entry point to the program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Entry point to the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                  .CreateDefaultBuilder(args)
                  .ConfigureLogging(logBuilder =>
                    {
                        logBuilder.AddConsole();
                        logBuilder.AddDebug();
                        logBuilder.AddEventSourceLogger();
                    })
                  .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
