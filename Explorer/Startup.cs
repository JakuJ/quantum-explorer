using System.Diagnostics.CodeAnalysis;
using Compiler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Explorer
{
    [SuppressMessage("Documentation", "SA1600", Justification = "Boilerplate")]
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                app.UseExceptionHandler("/Error")
                   .UseHttpsRedirection();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
               .UseStaticFiles()
               .UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");
                });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<ICompiler>(container => new QsCompiler(container.GetRequiredService<ILogger<QsCompiler>>()));
        }
    }
}
