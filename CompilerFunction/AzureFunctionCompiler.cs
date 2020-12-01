using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Compiler;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CompilerService
{
    /// <inheritdoc/>
    public class AzureFunctionCompiler : ICompiler
    {
        private static readonly string Endpoint;

        private static readonly HttpClient Client = new();

        private readonly ILogger log;

        static AzureFunctionCompiler()
        {
            string? endpoint = Environment.GetEnvironmentVariable("FUNCTION_ENDPOINT");
            Endpoint = endpoint ?? throw new Exception("FUNCTION_ENDPOINT environment variable not set. Cannot use Azure Functions.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionCompiler"/> class.
        /// </summary>
        /// <param name="log">An <see cref="ILogger"/> instance used for logging.</param>
        public AzureFunctionCompiler(ILogger log) => this.log = log;

        /// <inheritdoc/>
        public event EventHandler<string>? OnDiagnostics;

        /// <inheritdoc/>
        public event EventHandler<Dictionary<string, GateGrid>>? OnGrids;

        /// <inheritdoc/>
        public event EventHandler<string>? OnOutput;

        /// <inheritdoc/>
        public event EventHandler<List<OperationState>>? OnStatesRecorded;

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            var content = new StringContent(code);

            log.LogInformation($"Sending code to Azure Function at {Endpoint}");
            HttpResponseMessage response = await Client.PostAsync(Endpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string message = $"Got response code {response.StatusCode} from Azure Function.";
                log.LogError(message);
                log.LogError($"Response string: {responseString}");
                OnDiagnostics?.Invoke(this, $"There was an issue while processing your code. Try again later. (status: {response.StatusCode})");
                return;
            }

            Payload payload;

            try
            {
                JsonSerializerSettings settings = new() { MaxDepth = 128 };
                payload = JsonConvert.DeserializeObject<Payload>(responseString, settings)
                       ?? throw new Exception("Payload received from Azure Function was null");
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                log.LogError($"Response string: {responseString}");

                OnDiagnostics?.Invoke(this, "There was an issue while processing your code. Try again later.");
                return;
            }

            if (!string.IsNullOrEmpty(payload.Diagnostics))
            {
                OnDiagnostics?.Invoke(this, payload.Diagnostics);
            }

            if (!string.IsNullOrEmpty(payload.Output))
            {
                OnOutput?.Invoke(this, payload.Output);
            }

            if (payload.Grids != null)
            {
                OnGrids?.Invoke(this, payload.Grids);
            }

            if (payload.States != null)
            {
                OnStatesRecorded?.Invoke(this, payload.States);
            }
        }
    }
}
