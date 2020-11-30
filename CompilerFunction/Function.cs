using System.IO;
using System.Threading.Tasks;
using Compiler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CompilerService
{
    public static class Function
    {
        private static QsCompiler? compiler;

        [FunctionName("CompilerFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            if (compiler == null)
            {
                log.LogInformation($"Initializing a new {nameof(QsCompiler)} instance");
                compiler = new QsCompiler(log);
            }

            string code = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = new Payload();

            compiler.OnOutput += (_, s) => { payload.Output = s; };
            compiler.OnDiagnostics += (_, s) => { payload.Diagnostics = s; };
            compiler.OnGrids += (_, s) => { payload.Grids = s; };
            compiler.OnStatesRecorded += (_, s) => { payload.States = s; };

            await compiler.Compile(code);

            string message = JsonConvert.SerializeObject(payload);

            return new OkObjectResult(message);
        }
    }
}
