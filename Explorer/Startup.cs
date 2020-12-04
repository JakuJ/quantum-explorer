using System.Diagnostics.CodeAnalysis;
using Compiler;
using Compiler.AzureFunction;
using Compiler.AzureFunction.Connection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Explorer
{
    [SuppressMessage("Documentation", "SA1600", Justification = "Boilerplate")]
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IWebHostEnvironment env) => Env = env;

        private IWebHostEnvironment Env { get; }

        public void Configure(IApplicationBuilder app)
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton(_ => Env);

            if (Env.IsProduction())
            {
                services.AddScoped<ICompiler>(container =>
                {
                    var client = new AzureFunctionClient(container.GetRequiredService<ILogger<AzureFunctionClient>>());
                    return new AzureFunctionCompiler(client, container.GetRequiredService<ILogger<AzureFunctionCompiler>>());
                });
            }
            else
            {
                services.AddScoped<ICompiler>(container => new QsCompiler(container.GetRequiredService<ILogger<QsCompiler>>()));
            }
        }
    }
}
