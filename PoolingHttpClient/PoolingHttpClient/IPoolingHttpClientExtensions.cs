using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PoolingHttpClient
{
    /// <summary>
    /// IPoolingHttpClient extensions
    /// </summary>
    public static class IPoolingHttpClientExtensions
    {
        /// <summary>
        /// Execute HTTP GET
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpGetAsync(this IPoolingHttpClient client, string url, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpGetAsync(client, url, CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP GET
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="cancellationToken"></param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpGetAsync(this IPoolingHttpClient client, string url, CancellationToken cancellationToken, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            var requestUrl = new Uri(url);
            if (getParameters != null && getParameters.Count > 0)
            {
                requestUrl = new Uri($"{url}?{string.Join("&", getParameters.Select(s => $"{s.Key}={HttpUtility.UrlEncode(s.Value)}").ToArray())}");
            }
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            var response = await client.ExecuteRequestAsync(request, cancellationToken, timeout);
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }

        /// <summary>
        /// Execute HTTP POST
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="postData">POST data</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPostAsync(this IPoolingHttpClient client, string url, string postData, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpPostAsync(client, url, postData, Encoding.UTF8, "application/json", CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP POST
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="postData">POST data</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="mediaType">MIME</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPostAsync(this IPoolingHttpClient client, string url, string postData, Encoding encoding, string mediaType, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpPostAsync(client, url, postData, encoding, mediaType, CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP POST
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="postData">POST data</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="mediaType">MIME</param>
        /// <param name="cancellationToken"></param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPostAsync(this IPoolingHttpClient client, string url, string postData, Encoding encoding, string mediaType, CancellationToken cancellationToken, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            var requestUrl = new Uri(url);
            if (getParameters != null && getParameters.Count > 0)
            {
                requestUrl = new Uri($"{url}?{string.Join("&", getParameters.Select(s => $"{s.Key}={HttpUtility.UrlEncode(s.Value)}").ToArray())}");
            }
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Content = new StringContent(postData, encoding, mediaType);
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            var response = await client.ExecuteRequestAsync(request, cancellationToken, timeout);
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }

        /// <summary>
        /// Execute HTTP PUT
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="putData">PUT data</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPutAsync(this IPoolingHttpClient client, string url, string putData, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpPutAsync(client, url, putData, Encoding.UTF8, "application/json", CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP PUT
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="putData">PUT data</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="mediaType">MIME</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPutAsync(this IPoolingHttpClient client, string url, string putData, Encoding encoding, string mediaType, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpPutAsync(client, url, putData, encoding, mediaType, CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP PUT
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="putData">PUT data</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="mediaType">MIME</param>
        /// <param name="cancellationToken"></param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpPutAsync(this IPoolingHttpClient client, string url, string putData, Encoding encoding, string mediaType, CancellationToken cancellationToken, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            var requestUrl = new Uri(url);
            if (getParameters != null && getParameters.Count > 0)
            {
                requestUrl = new Uri($"{url}?{string.Join("&", getParameters.Select(s => $"{s.Key}={HttpUtility.UrlEncode(s.Value)}").ToArray())}");
            }
            var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            request.Content = new StringContent(putData, encoding, mediaType);
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            var response = await client.ExecuteRequestAsync(request, cancellationToken, timeout);
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }

        /// <summary>
        /// Execute HTTP DELETE
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpDeleteAsync(this IPoolingHttpClient client, string url, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            return await ExecuteHttpDeleteAsync(client, url, CancellationToken.None, getParameters, headers, timeout);
        }

        /// <summary>
        /// Execute HTTP DELETE
        /// </summary>
        /// <param name="client">HttpClient client</param>
        /// <param name="url">Request Url</param>
        /// <param name="cancellationToken"></param>
        /// <param name="getParameters">GET parameter</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> ExecuteHttpDeleteAsync(this IPoolingHttpClient client, string url, CancellationToken cancellationToken, IDictionary<string, string> getParameters = null, IDictionary<string, string> headers = null, int timeout = 30)
        {
            var requestUrl = new Uri(url);
            if (getParameters != null && getParameters.Count > 0)
            {
                requestUrl = new Uri($"{url}?{string.Join("&", getParameters.Select(s => $"{s.Key}={HttpUtility.UrlEncode(s.Value)}").ToArray())}");
            }
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            var response = await client.ExecuteRequestAsync(request, cancellationToken, timeout);
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }
    }
}
