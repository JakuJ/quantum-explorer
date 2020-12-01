using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Compiler.AzureFunction.Connection
{
    /// <inheritdoc />
    public class AzureFunctionClient : IHttpClient
    {
        private static readonly string Endpoint;

        private readonly ILogger log;

        private readonly HttpClient client = new();

        static AzureFunctionClient()
        {
            string? endpoint = Environment.GetEnvironmentVariable("FUNCTION_ENDPOINT");
            Endpoint = endpoint ?? throw new Exception("FUNCTION_ENDPOINT environment variable not set. Cannot use Azure Functions.");
        }

        /// <summary>Initializes a new instance of the <see cref="AzureFunctionClient"/> class.</summary>
        /// <param name="log">An <see cref="ILogger"/> instance used for logging.</param>
        public AzureFunctionClient(ILogger log) => this.log = log;

        /// <inheritdoc />
        public async Task<string?> MakeRequest(string code)
        {
            log.LogInformation($"Sending code to Azure Function at {Endpoint}");

            var content = new StringContent(code, Encoding.UTF8, "text/plain");
            HttpResponseMessage response = await client.PostAsync(Endpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseString;
            }

            string message = $"Got response code {response.StatusCode} from Azure Function.";
            log.LogError(message);
            log.LogError($"Response string: {responseString}");
            return null;
        }
    }
}
