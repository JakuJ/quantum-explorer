using System.IO;
using System.Threading.Tasks;
using Compiler.AzureFunction;
using Compiler.AzureFunction.Connection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Compiler.Tests
{
    /// <inheritdoc />
    /// <summary>A client which executes the Azure Function locally.</summary>
    public class LocalAzureFunctionClient : IHttpClient
    {
        /// <inheritdoc/>
        public async Task<string?> MakeRequest(string code, bool expanding)
        {
            // Prepare the request body as a Stream (Disposable!)
            await using Stream body = GenerateStreamFromString(code);

            // Create a HttpRequest
            DefaultHttpContext context = new();
            HttpRequest request = context.Request;
            request.Body = body;
            request.Headers.Add("x-expanding-operations", "yes");

            var result = await Function.Run(request, Helpers.ConsoleLogger) as OkObjectResult;
            return result?.Value as string;
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
