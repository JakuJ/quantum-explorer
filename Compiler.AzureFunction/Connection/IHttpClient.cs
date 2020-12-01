using System.Threading.Tasks;

namespace Compiler.AzureFunction.Connection
{
    /// <summary>
    /// Represents a client which can execute the compiler Azure Function.
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Make a request to the Azure Function and return the response.
        /// </summary>
        /// <param name="code">Code to send to the Azure Function.</param>
        /// <returns>Response from the Function.</returns>
        public Task<string?> MakeRequest(string code);
    }
}
