using System;
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
               .CreateDefaultBuilder(args)
               .ConfigureLogging((hostingContext, logBuilder) =>
                {
                    logBuilder.ClearProviders()
                              .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                              .AddConsole()
                              .AddDebug()
                              .AddEventSourceLogger()
                              .AddAzureWebAppDiagnostics();
                })
               .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseStaticWebAssets();

                    string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                    if (environment == "Development")
                    {
                        // the PORT variable is provided on Heroku
                        string? port = Environment.GetEnvironmentVariable("PORT");
                        if (port != null)
                        {
                            webBuilder.UseUrls($"http://*:{port}");
                        }
                    }
                });
    }
}
