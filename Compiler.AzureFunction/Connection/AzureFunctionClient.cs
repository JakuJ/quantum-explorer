using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Compiler.AzureFunction.Connection
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage] // TODO: Integration tests
    public class AzureFunctionClient : IHttpClient
    {
        private static readonly string Endpoint;
        private static readonly string? FunctionsKey;

        private readonly ILogger log;

        private readonly HttpClient client = new();

        static AzureFunctionClient()
        {
            Endpoint = Environment.GetEnvironmentVariable("FUNCTION_ENDPOINT")
                    ?? throw new Exception("FUNCTION_ENDPOINT environment variable not set. Cannot use Azure Functions.");

            FunctionsKey = Environment.GetEnvironmentVariable("FUNCTION_KEY");
        }

        /// <summary>Initializes a new instance of the <see cref="AzureFunctionClient"/> class.</summary>
        /// <param name="log">An <see cref="ILogger"/> instance used for logging.</param>
        public AzureFunctionClient(ILogger log) => this.log = log;

        /// <inheritdoc />
        public async Task<string?> MakeRequest(string code)
        {
            log.LogInformation($"Sending code to Azure Function at {Endpoint}");

            var content = new StringContent(code, Encoding.UTF8, "text/plain");

            if (FunctionsKey == null)
            {
                log.LogWarning("FUNCTION_KEY environment variable not set. Connection will be refused unless its authorization level is Anonymous.");
            }
            else
            {
                content.Headers.Add("x-functions-key", FunctionsKey);
            }

            try
            {
                HttpResponseMessage response = await client.PostAsync(Endpoint, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return responseString;
                }

                log.LogError($"Got response code {response.StatusCode} from Azure Function.");
                return null;
            }
            catch (HttpRequestException e)
            {
                log.LogError(e.Message);
                return null;
            }
        }
    }
}
