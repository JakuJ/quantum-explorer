using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Compiler.AzureFunction.Connection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Compiler.AzureFunction
{
    /// <inheritdoc/>
    public class AzureFunctionCompiler : ICompiler
    {
        private readonly IHttpClient client;
        private readonly ILogger log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionCompiler"/> class.
        /// </summary>
        /// <param name="client">An <see cref="IHttpClient"/> instance used for making connections.</param>
        /// <param name="log">An <see cref="ILogger"/> instance used for logging.</param>
        public AzureFunctionCompiler(IHttpClient client, ILogger log)
        {
            this.client = client;
            this.log = log;
        }

        /// <inheritdoc/>
        public event EventHandler<string>? OnDiagnostics;

        /// <inheritdoc/>
        public event EventHandler<Dictionary<string, List<GateGrid>>>? OnGrids;

        /// <inheritdoc/>
        public event EventHandler<string>? OnOutput;

        /// <inheritdoc/>
        public event EventHandler<List<OperationState>>? OnStatesRecorded;

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            string? responseString = await client.MakeRequest(code);

            if (responseString == null)
            {
                OnDiagnostics?.Invoke(this, "There has been an issue while processing your request.\nTry again later.");
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
                OnDiagnostics?.Invoke(this, "There has been an issue while processing your request.\nTry again later.");
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
