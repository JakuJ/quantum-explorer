using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Compiler;
using Compiler.AzureFunction;
using Compiler.AzureFunction.Connection;
using DatabaseHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
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
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

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
               .UseResponseCompression()
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

            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

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

            services.AddDbContext<CodeDbContext>(options =>
                                                     options.UseSqlServer(
                                                         Configuration.GetConnectionString("DatabaseConnection"),
                                                         x => x.MigrationsAssembly("DatabaseHandler")));
            services.AddScoped<ICodeDatabaseHandler, CodeDatabaseHandler>();
            services.AddScoped<Common.CellMenusNotifier>();
        }
    }
}
