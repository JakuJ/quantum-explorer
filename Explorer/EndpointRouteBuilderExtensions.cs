using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Explorer
{
    internal static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapStreamJsonRpc(this IEndpointRouteBuilder endpoints, string pattern)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            RequestDelegate pipeline = endpoints.CreateApplicationBuilder()
               .UseMiddleware<StreamJsonRpcMiddleware>()
               .Build();

            return endpoints.Map(pattern, pipeline);
        }
    }
}
