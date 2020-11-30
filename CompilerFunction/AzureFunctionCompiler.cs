using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Compiler;
using Newtonsoft.Json;

namespace CompilerService
{
    /// <inheritdoc/>
    public class AzureFunctionCompiler : ICompiler
    {
        private static readonly HttpClient Client = new ();

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

            // HttpResponseMessage response = await Client.PostAsync("http://localhost:7071/api/CompilerFunction", content);
            HttpResponseMessage response = await Client.PostAsync("https://qs-compiler.azurewebsites.net/api/CompilerFunction", content);

            string responseString = await response.Content.ReadAsStringAsync();

            Payload payload = JsonConvert.DeserializeObject<Payload>(responseString, new JsonSerializerSettings
            {
                MaxDepth = 128,
            })!; // either it throws, or it's not null

            OnDiagnostics?.Invoke(this, payload.Diagnostics!);
            OnOutput?.Invoke(this, payload.Output!);
            OnGrids?.Invoke(this, payload.Grids!);
            OnStatesRecorded?.Invoke(this, payload.States!);
        }
    }
}
