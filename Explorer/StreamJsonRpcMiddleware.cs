using Microsoft.AspNetCore.Http;
using Microsoft.Quantum.QsLanguageServer;
using StreamJsonRpc;
using System.Threading.Tasks;

namespace Explorer
{
    internal class StreamJsonRpcMiddleware
    {
        public StreamJsonRpcMiddleware(RequestDelegate next) { }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(webSocket);

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
