using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Compiler.AzureFunction
{
    /// <summary>
    /// An Azure Function responsible for performing Q# code compilation and simulation.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// Execute the Azure Function on a POST request made to the corresponding endpoint.
        /// </summary>
        /// <param name="req">A HTTP request that triggered the function.</param>
        /// <param name="log">An <see cref="ILogger"/> instance provided by Azure.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("CompilerFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            // Multiple instances of the Function on the same server will share static QsCompiler cache.
            QsCompiler compiler = new(log);

            string code = await new StreamReader(req.Body).ReadToEndAsync();

            Payload payload = new();

            compiler.OnOutput += (_, s) => { payload.Output = s; };
            compiler.OnDiagnostics += (_, s) => { payload.Diagnostics = s; };
            compiler.OnGrids += (_, s) => { payload.Grids = s; };
            compiler.OnStatesRecorded += (_, s) => { payload.States = s; };

            await compiler.Compile(code, req.Headers.ContainsKey("x-expanding-operations"));

            string message = JsonConvert.SerializeObject(payload);

            return new OkObjectResult(message);
        }
    }
}
