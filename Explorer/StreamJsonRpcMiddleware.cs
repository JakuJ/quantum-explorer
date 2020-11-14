using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Quantum.QsLanguageServer;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace Explorer
{
    internal class StreamJsonRpcMiddleware
    {
        public StreamJsonRpcMiddleware(RequestDelegate next) { }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                WebSocketMessageHandler jsonRpcMessageHandler = new(webSocket);

                QsLanguageServer server = new QsLanguageServer(jsonRpcMessageHandler);
                server.WaitForShutdown();
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
