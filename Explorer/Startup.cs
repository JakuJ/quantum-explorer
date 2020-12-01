using System.Diagnostics.CodeAnalysis;
using Compiler;
using CompilerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Explorer
{
    [SuppressMessage("Documentation", "SA1600", Justification = "Boilerplate")]
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IWebHostEnvironment env) => Env = env;

        private static IWebHostEnvironment? Env { get; set; }

        public static void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting()
               .UseStaticFiles()
               .UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");
                });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<IWebHostEnvironment>(_ => Env!); // always set in the constructor

            if (Env.IsProduction())
            {
                services.AddScoped<ICompiler>(container => new AzureFunctionCompiler(container.GetRequiredService<ILogger<AzureFunctionCompiler>>()));
            }
            else
            {
                services.AddScoped<ICompiler>(container => new QsCompiler(container.GetRequiredService<ILogger<QsCompiler>>()));
            }
        }
    }
}
