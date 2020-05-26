using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PoolingHttpClient
{
    /// <summary>
    /// Default PoolingHttpClient
    /// </summary>
    public class DefaultPoolingHttpClient : IPoolingHttpClient, IDisposable
    {
        private ConcurrentDictionary<string, HttpClientConnectionPool> _poolDict = new ConcurrentDictionary<string, HttpClientConnectionPool>();
        private Dictionary<string, int> _connectionLimitDict = new Dictionary<string, int>();
        private volatile int _isDisposed = 0;
        private Timer cleanIdleHttpClientTimer;

        /// <summary>
        /// Default PoolingHttpClient
        /// </summary>
        public DefaultPoolingHttpClient()
        {
            cleanIdleHttpClientTimer = new Timer(ClearIdleHttpClient);
            cleanIdleHttpClientTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Finalize DefaultPoolingHttpClient
        /// </summary>
        ~DefaultPoolingHttpClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Default connection limit（Default value：System.Net.ServicePointManager.DefaultConnectionLimit）
        /// </summary>
        public static int DefaultConnectionLimit
        {
            get
            {
                return ServicePointManager.DefaultConnectionLimit;
            }
            set
            {
                ServicePointManager.DefaultConnectionLimit = value;
            }
        }

        /// <summary>
        /// Max connection idle seconds（Default value：10）
        /// </summary>
        public int MaxConnectionIdleSeconds { get; set; }

        /// <summary>
        /// Debug enabled or not
        /// </summary>
        public bool DebugEnabled { get; set; }

        /// <summary>
        /// Set connection limit by uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="connectionLimit"></param>
        public void SetConnectionLimit(Uri uri, int connectionLimit)
        {
            var poolKey = GetBaseAddress(uri);
            if (_connectionLimitDict.ContainsKey(poolKey))
            {
                _connectionLimitDict[poolKey] = connectionLimit;
            }
            else
            {
                _connectionLimitDict.Add(poolKey, connectionLimit);
            }
        }

        /// <summary>
        /// Get connection limit by uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public int GetConnectionLimit(Uri uri)
        {
            var poolKey = GetBaseAddress(uri);
            if (_connectionLimitDict.ContainsKey(poolKey))
            {
                return _connectionLimitDict[poolKey];
            }
            else
            {
                return DefaultConnectionLimit;
            }
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken, int timeout = 30)
        {
            if (_isDisposed == 1)
            {
                throw new ObjectDisposedException(nameof(DefaultPoolingHttpClient));
            }
            var requestUrl = request.RequestUri;
            return await GetPool(requestUrl).ProcessRequestAsync(async (httpClient, state) =>
            {
                var requestData = (ExecuteRequestData)state;
                try
                {
                    if (httpClient.BaseAddress == null)
                    {
                        httpClient.BaseAddress = new Uri(GetBaseAddress(requestData.Request.RequestUri));
                        httpClient.Timeout = new TimeSpan(0, 0, requestData.Timeout);
                    }
                    return await httpClient.SendAsync(requestData.Request, requestData.CancellationToken);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"SendRequestAsync {requestData.Request.RequestUri} fail: {ex.Message}", ex);
                }
            }, new ExecuteRequestData(request, cancellationToken, timeout)).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private string GetBaseAddress(Uri uri)
        {
            return $"{uri.Scheme}://{uri.Authority}";
        }

        private HttpClientConnectionPool GetPool(Uri uri)
        {
            var poolKey = GetBaseAddress(uri);
            if (!_poolDict.ContainsKey(poolKey))
            {
                var connectionLimit = DefaultConnectionLimit;
                if (_connectionLimitDict.ContainsKey(poolKey))
                {
                    connectionLimit = _connectionLimitDict[poolKey];
                }
                _poolDict.TryAdd(poolKey, new HttpClientConnectionPool(poolKey, connectionLimit, MaxConnectionIdleSeconds) { DebugEnabled = DebugEnabled });
            }
            return _poolDict[poolKey];
        }

        private void ClearIdleHttpClient(object state)
        {
            cleanIdleHttpClientTimer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                foreach (var key in _poolDict.Keys)
                {
                    var pool = _poolDict[key];
                    pool.CleanIdleHttpClient();
                }
            }
            finally
            {
                cleanIdleHttpClientTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                foreach (var key in _poolDict.Keys)
                {
                    if (_poolDict.TryRemove(key, out HttpClientConnectionPool pool))
                    {
                        try
                        {
                            pool.Dispose();
                        }
                        catch { }
                    }
                }
            }
        }

        private class ExecuteRequestData
        {
            public ExecuteRequestData(HttpRequestMessage request, CancellationToken cancellationToken, int timeout)
            {
                Request = request;
                CancellationToken = cancellationToken;
                Timeout = timeout;
            }

            public HttpRequestMessage Request { get; private set; }

            public CancellationToken CancellationToken { get; private set; }

            public int Timeout { get; private set; }
        }
    }
}
