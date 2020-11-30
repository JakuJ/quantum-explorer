using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Compiler;
using Newtonsoft.Json;

namespace CompilerService
{
    public class CompilerFunctionFacade : ICompiler
    {
        private static readonly HttpClient client = new HttpClient();

        public event EventHandler<string>? OnDiagnostics;

        public event EventHandler<Dictionary<string, GateGrid>>? OnGrids;

        public event EventHandler<string>? OnOutput;

        public event EventHandler<List<OperationState>>? OnStatesRecorded;

        public async Task Compile(string code)
        {
            var content = new StringContent(code);

            var response = await client.PostAsync("http://localhost:7071/api/CompilerFunction", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            var payload = JsonConvert.DeserializeObject<Payload>(responseString, new JsonSerializerSettings
            {
                MaxDepth = 128
            });

            OnDiagnostics?.Invoke(this, payload.Diagnostics);
            OnOutput?.Invoke(this, payload.Output);
            OnGrids?.Invoke(this, payload.Grids);
            OnStatesRecorded?.Invoke(this, payload.States);
        }
    }
}
