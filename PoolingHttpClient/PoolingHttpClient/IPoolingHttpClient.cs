using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PoolingHttpClient
{
    /// <summary>
    /// PoolingHttpClient interface
    /// </summary>
    public interface IPoolingHttpClient
    {
        /// <summary>
        /// Execute request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken, int timeout = 30);
    }
}
